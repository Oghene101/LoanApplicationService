using FluentValidation;
using LoanApplication.Application.Common.Contracts;
using LoanApplication.Application.Common.Contracts.Abstractions;
using LoanApplication.Application.Common.Contracts.Abstractions.Security;
using LoanApplication.Application.Common.Exceptions;
using LoanApplication.Application.Extensions;
using LoanApplication.Domain.Constants;
using LoanApplication.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SerilogTimings;

namespace LoanApplication.Application.Features.Auth.Commands;

public static class SignUp
{
    public sealed record Command(
        string FirstName,
        string LastName,
        string Email,
        string Password) : IRequest<Result<Guid>>;

    public sealed class Handler(
        UserManager<User> userManager,
        IBackgroundTaskQueue queue,
        IUnitOfWork uOw,
        ILogger<Handler> logger) : IRequestHandler<Command, Result<Guid>>
    {
        private static readonly string HandlerName = typeof(Handler).GetOuterAndInnerName();

        public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = Operation.Begin("{HandlerName} with Email: {Email}", HandlerName,
                request.Email);

            await uOw.BeginTransactionAsync(cancellationToken);
            var user = request.ToEntity();

            try
            {
                var result = await userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                    throw ApiException.BadRequest(
                        result.Errors.Select(e => new Error(e.Code, e.Description))
                            .ToArray());

                result = await userManager.AddToRoleAsync(user, Roles.User);
                if (!result.Succeeded)
                {
                    throw ApiException.BadRequest(
                        result.Errors.Select(e => new Error(e.Code, e.Description))
                            .ToArray());
                }

                await uOw.CommitTransactionAsync(cancellationToken);
            }
            catch (Exception)
            {
                op.Abandon();
                await uOw.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            await SendEmailAsync(op, user, cancellationToken);

            op.Complete();
            return Result.Success(user.Id);
        }

        private async Task SendEmailAsync(Operation op, User user, CancellationToken cancellationToken = default)
        {
            try
            {
                await queue.QueueBackgroundWorkItemAsync((async (sp, ct) =>
                {
                    try
                    {
                        var auth = sp.GetRequiredService<IAuthService>();
                        await auth.SendEmailConfirmationAsync(user, ct);
                    }
                    catch (Exception ex)
                    {
                        {
                            // log and swallow, so it doesn't crash the worker
                            logger.LogError(ex,
                                "Error occured while sending email confirmation for user with email: {EMail}",
                                user.Email);
                        }
                    }
                }, nameof(SendEmailAsync)), cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                op.Abandon();
                logger.LogError(ex, "Error occured while queueing background work item.");
            }
        }
    }

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one digit")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
        }
    }
}