namespace LoanApplication.Application.Common.Contracts.Abstractions.Repositories;

public interface ILoanApplicationRepository
{
    Task<Domain.Entities.LoanApplication?> GetLoanApplicationByIdAsync(
        Guid id);

    Task<(IEnumerable<Domain.Entities.LoanApplication> LoanApplications, int TotalCount)>
        GetPendingLoanApplicationsAsync(
            int pageNumber, int pageSize,
            DateTimeOffset? startDate, DateTimeOffset? endDate);
}