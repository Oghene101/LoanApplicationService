using LoanApplication.Application.Common.Contracts.Abstractions.Mailing;
using LoanApplication.Domain.Entities;
using LoanApplication.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace LoanApplication.Infrastructure.Services;

internal sealed class EmailTemplates(
    IOptions<LoanTenureSettings> loanTenure) : IEmailTemplates
{
    private readonly LoanTenureSettings _loanTenure = loanTenure.Value;

    public string EmailConfirmation(User user, string confirmationLink)
    {
        return $"""
                <p>Hello {user.FirstName},</p>
                <p>Please confirm your email by clicking the link below:</p>
                <p><a href='{confirmationLink}'>Confirm Email</a></p>
                <p>This link will expire shortly for your security.</p>
                """;
    }

    public string LoanApprovalEmail(
        string customerName,
        decimal amount,
        int tenure,
        DateTimeOffset repaymentStartDate)
    {
        return $"""

                Dear {customerName},

                We are pleased to inform you that your loan application has been approved.

                LOAN DETAILS
                - Loan Amount: {amount:C}
                - Tenor: {tenure}
                - Interest Rate: {_loanTenure.InterestRate}%
                - Repayment Start Date: {repaymentStartDate:dddd, MMMM dd, yyyy}

                Your approved funds will be disbursed to your account shortly. A detailed repayment schedule will also be shared with you.

                If you have any questions, our support team is available to assist you.

                Thank you for choosing our services.

                Warm regards,
                Loan Support Team

                """;
    }

    public string LoanRejectionEmail(string customerName)
    {
        return $"""

                Dear {customerName},

                Thank you for applying for a loan with us.

                After a thorough review, we regret to inform you that we are unable to approve your loan application at this time.

                This decision was based on our internal eligibility criteria. Please note that this does not prevent you from applying again in the future.

                If you require clarification or further assistance, feel free to contact our support team.

                Kind regards,
                Loan Support Team

                """;
    }
}