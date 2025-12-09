namespace LoanApplication.Application.Common.Contracts;

public static class LoanApplication
{
    public record ApplyForLoanRequest(decimal Amount, int Tenure, string Purpose);
}