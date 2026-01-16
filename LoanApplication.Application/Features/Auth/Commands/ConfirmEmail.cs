using FluentValidation;
using LoanApplication.Application.Common.Contracts;
using LoanApplication.Application.Common.Exceptions;
using LoanApplication.Application.Extensions;
using LoanApplication.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SerilogTimings;

namespace LoanApplication.Application.Features.Auth.Commands;

public static class ConfirmEmail
{
    public sealed record Command(string Email, string Token) : IRequest<Result<string>>;

    public sealed class Handler(
        UserManager<User> userManager) : IRequestHandler<Command, Result<string>>
    {
        private static readonly string HandlerName = typeof(Handler).GetOuterAndInnerName();

        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var email = Uri.UnescapeDataString(request.Email);
            using var op = Operation.Begin("{HandlerName} with Email: {Email}", HandlerName,
                email);

            var token = Uri.UnescapeDataString(request.Token);

            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                op.Abandon();
                throw ApiException.NotFound(new Error("Auth.Error", $"User with email '{email}' not found."));
            }

            if (user.EmailConfirmed)
            {
                op.Complete();
                return Result.Success("Your email has already been confirmed.");
            }

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                op.Abandon();
                throw ApiException.BadRequest(result.Errors.Select(e => new Error(e.Code, e.Description))
                    .ToArray());
            }

            op.Complete();
            return Result.Success("Your email has been confirmed.");
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