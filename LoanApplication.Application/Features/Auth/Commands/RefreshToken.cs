using System.Security.Claims;
using FluentValidation;
using LoanApplication.Application.Common.Contracts;
using LoanApplication.Application.Common.Contracts.Abstractions;
using LoanApplication.Application.Common.Exceptions;
using LoanApplication.Application.Extensions;
using LoanApplication.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SerilogTimings;

namespace LoanApplication.Application.Features.Auth.Commands;

public static class RefreshToken
{
    public sealed record Command(string AccessToken, string RefreshToken) : IRequest<Result<Jwt.GenerateTokenResponse>>;

    public sealed class Handler(
        IJwtService jwt,
        UserManager<User> userManager,
        IUnitOfWork uOw) : IRequestHandler<Command, Result<Jwt.GenerateTokenResponse>>
    {
        private static readonly string HandlerName = typeof(Handler).GetOuterAndInnerName();

        public async Task<Result<Jwt.GenerateTokenResponse>> Handle(Command request,
            CancellationToken cancellationToken)
        {
            using var op = Operation.Begin("{HandlerName} with Access Token: {AccessToken}", HandlerName,
                request.AccessToken);

            ClaimsPrincipal principal;
            ClaimsIdentity identity;
            try
            {
                principal = jwt.GetPrincipalFromExpiredToken(request.AccessToken);
            }
            catch (Exception)
            {
                op.Abandon();
                throw ApiException.Unauthorized(new Error("Auth.Error", "Invalid token"));
            }

            var email = principal.FindFirstValue(ClaimTypes.Email);

            var user = await userManager.FindByEmailAsync(email!);
            if (user is null)
            {
                op.Abandon();
                throw ApiException.Unauthorized(new Error("Auth.Error", "Invalid token"));
            }

            var refreshToken = await uOw.RefreshTokensReadRepository.GetRefreshTokenAsync(request.RefreshToken);
            if (refreshToken is null || refreshToken.UserId != user.Id)
            {
                op.Abandon();
                throw ApiException.Unauthorized(new Error("Auth.Error", "Invalid token"));
            }

            var roles = await userManager.GetRolesAsync(user);
            var token = await jwt.GenerateToken(new Jwt.GenerateTokenRequest(user, roles), cancellationToken);

            // todo: update refresh token auditables and mark as used & revoked
            op.Complete();
            return Result.Success(token);
        }
    }

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty().WithMessage("Access token is required");

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token required")
                .MaximumLength(44).WithMessage("Refresh token is invalid");
        }
    }
}