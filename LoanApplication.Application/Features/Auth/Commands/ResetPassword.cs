using FluentValidation;
using LoanApplication.Application.Common.Contracts;
using LoanApplication.Application.Common.Exceptions;
using LoanApplication.Application.Extensions;
using LoanApplication.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SerilogTimings;

namespace LoanApplication.Application.Features.Auth.Commands;

public static class ResetPassword
{
    public sealed record Command(string Email, string Token, string NewPassword) : IRequest<Result<string>>;

    public sealed class Handler(
        UserManager<User> userManager) : IRequestHandler<Command, Result<string>>
    {
        private static readonly string HandlerName = typeof(Handler).GetOuterAndInnerName();

        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = Operation.Begin("{HandlerName} with Email: {Email}", HandlerName,
                request.Email);

            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                op.Abandon();
                throw ApiException.NotFound(new Error("Auth.Error", $"User with email '{request.Email}' not found"));
            }

            var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                op.Abandon();
                throw ApiException.BadRequest(result.Errors.Select(e => new Error(e.Code, e.Description))
                    .ToArray());
            }

            op.Complete();
            return Result.Success("Your new password has been set.");
        }
    }

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Authentication required");
        }
    }
}