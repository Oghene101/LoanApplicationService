using FluentValidation;
using LoanApplication.Application.Common.Contracts;
using LoanApplication.Application.Common.Contracts.Abstractions;
using LoanApplication.Application.Common.Exceptions;
using LoanApplication.Application.Extensions;
using LoanApplication.Domain.Entities;
using LoanApplication.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LoanApplication.Application.Features.LoanApplication.Commands;

public static class ApplyForLoan
{
    public record Command(
        decimal Amount,
        int Tenure,
        string Purpose) : IRequest<Result<string>>;

    public class Handler(
        IAuthService authService,
        UserManager<User> userManager,
        IUnitOfWork uOw) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            await uOw.BeginTransactionAsync(cancellationToken);
            try
            {
                var email = authService.GetSignedInUserEmail();
                var user = await userManager.FindByEmailAsync(email);
                if (user is null) throw ApiException.NotFound(new Error("LoanApplication.Error", "User not found"));

                var loanApplication = request.ToEntity(user);
                var loanApplicationHistory = new LoanApplicationHistory
                {
                    ApplicationStatus = LoanApplicationStatus.Pending,
                    LoanApplicationId = loanApplication.Id,
                    CreatedBy = $"{user.FirstName} {user.LastName}",
                    UpdatedBy = $"{user.FirstName} {user.LastName}",
                };

                await uOw.LoanApplicationsWriteRepository.AddAsync(loanApplication, cancellationToken);
                await uOw.LoanApplicationHistoryWriteRepository.AddAsync(loanApplicationHistory, cancellationToken);

                await uOw.CommitTransactionAsync(cancellationToken);

                return Result.Success("Your loan application was successful");
            }
            catch (Exception)
            {
                await uOw.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Amount)
                .Must(x => x >= 5_000).WithMessage("Amount must not be less than 5,000.");

            RuleFor(x => x.Tenure)
                .Must(x => x > 0).WithMessage("Tenure must be greater than 0.");

            RuleFor(x => x.Purpose)
                .NotEmpty().WithMessage("Purpose is required.")
                .MaximumLength(500);
        }
    }
}