using CharityDonationsApp.Application.Common.Contracts;
using FluentValidation;
using LoanApplication.Application.Common.Contracts;
using LoanApplication.Application.Common.Contracts.Abstractions;
using LoanApplication.Application.Common.Exceptions;
using LoanApplication.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LoanApplication.Application.Features.Auth.Commands;

public static class ForgotPassword
{
    public record Command(string Email) : IRequest<Result<string>>;

    public class Handler(
        UserManager<User> userManager,
        IAuthService auth) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
                throw ApiException.NotFound(new Error("Auth.Error", $"User with email '{request.Email}' not found"));

            await auth.SendForgotPasswordEmailAsync(user, cancellationToken);
            return Result.Success("A link has been sent to your email address to reset your password.");
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");
        }
    }
}