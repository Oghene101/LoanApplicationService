using LoanApplication.Domain.Enums;

namespace LoanApplication.Application.Common.Contracts;

public static class LoanApplicationAdmin
{
    public record GetPendingLoanApplicationsRequest(
        DateTimeOffset? StartDate,
        DateTimeOffset? EndDate,
        int PageNumber = 1,
        int PageSize = 10);

    public record GetPendingLoanApplicationsResponse(
        Guid LoanApplicationId,
        string FirstName,
        string LastName,
        string Email,
        decimal Amount,
        int Tenure,
        string Purpose,
        DateTimeOffset ApplicationDate);

    public record ReviewLoanApplicationRequest(
        Guid LoanApplicationId,
        string Email,
        string FirstName,
        string LastName,
        LoanApplicationStatus LoanApplicationStatus,
        string? Comment);
}