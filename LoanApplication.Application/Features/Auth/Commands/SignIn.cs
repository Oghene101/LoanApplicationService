using FluentValidation;
using LoanApplication.Application.Common.Contracts;
using LoanApplication.Application.Common.Contracts.Abstractions;
using LoanApplication.Application.Common.Contracts.Abstractions.Security;
using LoanApplication.Application.Common.Exceptions;
using LoanApplication.Application.Extensions;
using LoanApplication.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SerilogTimings;

namespace LoanApplication.Application.Features.Auth.Commands;

public static class SignIn
{
    public sealed record Command(string Email, string Password)
        : IRequest<Result<Common.Contracts.Auth.SignInResponse>>;

    public sealed class Handler(
        UserManager<User> userManager,
        IJwtService jwt,
        IAuthService auth,
        IUtilityService utility) : IRequestHandler<Command, Result<Common.Contracts.Auth.SignInResponse>>
    {
        private static readonly string HandlerName = typeof(Handler).GetOuterAndInnerName();
        private const string SignInTokenCacheKey = "UserAuthToken";
        private const string UserRolesCacheKey = "UserRoles";

        public async Task<Result<Common.Contracts.Auth.SignInResponse>> Handle(Command request,
            CancellationToken cancellationToken)
        {
            using var op = Operation.Begin("{HandlerName} with Email: {Email}", HandlerName,
                request.Email);

            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                op.Abandon();
                throw ApiException.BadRequest(new Error("Auth.Error", "Incorrect email or password"));
            }

            if (await userManager.IsLockedOutAsync(user))
            {
                op.Abandon();
                throw ApiException.BadRequest(new Error("Auth.Error",
                    $"Your account has been locked for {(user.LockoutEnd! - DateTimeOffset.UtcNow).Value.TotalSeconds} seconds. Try again later"));
            }

            await auth.CheckPassword(new Authentication.CheckPasswordRequest(user, request.Password));

            if (user.LockoutCount != 0)
            {
                user.LockoutCount = 0;
            }

            await userManager.ResetAccessFailedCountAsync(user);

            var signInTokenCacheKey = user.Email + SignInTokenCacheKey;
            var userRolesCacheKey = user.Email + UserRolesCacheKey;

            if (!utility.TryGetInMemoryCacheValue(signInTokenCacheKey,
                    out Jwt.GenerateTokenResponse? generateTokenResponse) ||
                !utility.TryGetInMemoryCacheValue(userRolesCacheKey, out IList<string>? roles))
            {
                roles = await userManager.GetRolesAsync(user);

                generateTokenResponse =
                    await jwt.GenerateToken(new Jwt.GenerateTokenRequest(user, roles), cancellationToken);

                var absoluteExpirationRelativeToNow = TimeSpan.FromMinutes(generateTokenResponse.ExpireMinutes);
                utility.SetInMemoryCache(signInTokenCacheKey, generateTokenResponse, absoluteExpirationRelativeToNow);
                utility.SetInMemoryCache(userRolesCacheKey, roles, absoluteExpirationRelativeToNow);
            }

            op.Complete();
            return Result.Success(new Common.Contracts.Auth.SignInResponse(user.Id, roles!, generateTokenResponse!));
        }
    }

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password is not valid")
                .Matches("[A-Z]").WithMessage("Password is not valid")
                .Matches("[a-z]").WithMessage("Password is not valid")
                .Matches("[0-9]").WithMessage("Password is not valid")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password is not valid");
        }
    }
}