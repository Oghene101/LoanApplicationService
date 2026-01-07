using LoanApplication.Domain.Entities;

namespace LoanApplication.Application.Common.Contracts.Abstractions.Mailing;

public interface IEmailTemplates
{
    string EmailConfirmation(User user, string confirmationLink);

    string LoanApprovalEmail(
        string customerName,
        decimal amount,
        int tenure,
        DateTimeOffset repaymentStartDate);

    string LoanRejectionEmail(string customerName);
}