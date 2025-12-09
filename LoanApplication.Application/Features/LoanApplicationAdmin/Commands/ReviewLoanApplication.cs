using FluentValidation;
using LoanApplication.Application.Common.Contracts;
using LoanApplication.Application.Common.Contracts.Abstractions;
using LoanApplication.Application.Common.Exceptions;
using LoanApplication.Domain.Entities;
using LoanApplication.Domain.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace LoanApplication.Application.Features.LoanApplicationAdmin.Commands;

public static class ReviewLoanApplication
{
    public record Command(
        Guid LoanApplicationId,
        string Email,
        string FirstName,
        string LastName,
        LoanApplicationStatus LoanApplicationStatus,
        string? Comment) : IRequest<Result<string>>;

    public class Handler(
        IAuthService auth,
        IEmailTemplates emailTemplates,
        IUnitOfWork uOw,
        IBackgroundTaskQueue backgroundTaskQueue,
        ILogger<Handler> logger) : IRequestHandler<Command, Result<string>>
    {
        private static readonly string Separator = new('*', 110);

        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                await uOw.BeginTransactionAsync(cancellationToken);

                if (request.LoanApplicationStatus is LoanApplicationStatus.Pending)
                    return Result.Success($"You have successfully {request.LoanApplicationStatus.ToString()} loan");

                var adminName = auth.GetSignedInUserName();

                var loanApplication =
                    await uOw.LoanApplicationsReadRepository.GetLoanApplicationByIdAsync(request.LoanApplicationId);

                if (loanApplication is null)
                    throw ApiException.NotFound(new Error("LoanApplicationAdmin.Error",
                        "Loan Application not found"));

                loanApplication.ApplicationStatus = request.LoanApplicationStatus;
                uOw.LoanApplicationsWriteRepository.Update(loanApplication,
                    x => x.ApplicationStatus);

                LoanApplicationHistory loanApplicationHistory;
                try
                {
                    loanApplicationHistory = new LoanApplicationHistory
                    {
                        ApplicationStatus = request.LoanApplicationStatus,
                        Comment = request.Comment,
                        LoanApplicationId = loanApplication.Id,
                        CreatedBy = adminName,
                        UpdatedBy = adminName,
                    };
                }
                catch (ValidationException ex)
                {
                    throw ApiException.BadRequest(new Error("LoanApplicationAdmin.Error", ex.Message));
                }

                await uOw.LoanApplicationHistoryWriteRepository.AddAsync(loanApplicationHistory, cancellationToken);

                await uOw.CommitTransactionAsync(cancellationToken);

                var customerName = $"{request.FirstName} {request.LastName}";
                var loanApplicationStatus = request.LoanApplicationStatus.ToString().ToLower();

                await SendEmailAsync(request.LoanApplicationStatus, customerName, loanApplication.Amount,
                    loanApplication.Tenure, request.Email, cancellationToken);

                return Result.Success($"You have successfully {loanApplicationStatus} {customerName}'s loan");
            }
            catch (Exception)
            {
                await uOw.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        private async Task SendEmailAsync(LoanApplicationStatus loanApplicationStatus, string customerName,
            decimal amount,
            int tenure, string email, CancellationToken cancellationToken = default)
        {
            var body = loanApplicationStatus switch
            {
                LoanApplicationStatus.Approved => emailTemplates.LoanApprovalEmail(customerName,
                    amount, tenure, DateTimeOffset.UtcNow.AddHours(48)),

                LoanApplicationStatus.Rejected => emailTemplates.LoanRejectionEmail(customerName),
            };

            try
            {
                await backgroundTaskQueue.QueueBackgroundWorkItemAsync(async (sp, ct) =>
                {
                    try
                    {
                        var emailService = sp.GetRequiredService<IEmailService>();
                        await emailService.SendAsync(customerName, $"{email}", "Loan Application Review", body,
                            ct);
                    }
                    catch (Exception ex)
                    {
                        // log and swallow, so it doesn't crash the worker
                        logger.LogError("""
                                        {Separator}
                                        Error occured while sending loan application review email to loan applicant

                                        Exception Message: {Message}

                                        Exception Type: {ExceptionType}
                                        {Separator}

                                        Stack Trace: {StackTrace}

                                        """, Separator, ex.Message,
                            ex.GetType().FullName ?? ex.GetType().Name, ex.StackTrace, Separator);
                    }
                }, cancellationToken);
            }
            catch (InvalidOperationException exception)
            {
                logger.LogError("""
                                {Separator} 

                                Exception Message: {Message}

                                {Separator}
                                """, Separator, exception.Message, Separator);
            }
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email must be a valid email address.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

            RuleFor(x => x.LoanApplicationStatus)
                .IsInEnum().WithMessage("Invalid loan application review.");
        }
    }
}