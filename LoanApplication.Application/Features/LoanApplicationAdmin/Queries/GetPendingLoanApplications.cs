using LoanApplication.Application.Common.Contracts;
using LoanApplication.Application.Common.Contracts.Abstractions;
using LoanApplication.Application.Common.Filters;
using LoanApplication.Application.Extensions;
using LoanApplication.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using GetPendingLoanApplicationsResponse =
    LoanApplication.Application.Common.Contracts.LoanApplicationAdmin.GetPendingLoanApplicationsResponse;

namespace LoanApplication.Application.Features.LoanApplicationAdmin.Queries;

public static class GetPendingLoanApplications
{
    public record Query(
        PaginationFilter PaginationFilter,
        DateRangeFilter DateRangeFilter)
        : IRequest<Result<PaginatorVm<IEnumerable<GetPendingLoanApplicationsResponse>>>>;

    public class Handler(
        UserManager<User> userManager,
        IUnitOfWork uOw) : IRequestHandler<Query, Result<PaginatorVm<IEnumerable<GetPendingLoanApplicationsResponse>>>>
    {
        public async
            Task<Result<PaginatorVm<IEnumerable<GetPendingLoanApplicationsResponse>>>> Handle(Query request,
                CancellationToken cancellationToken)
        {
            var pageNumber = request.PaginationFilter.PageNumber;
            var pageSize = request.PaginationFilter.PageSize;
            var startDate = request.DateRangeFilter.EffectiveStartDate;
            var endDate = request.DateRangeFilter.EffectiveEndDate;

            var (loanApplications, totalCount) =
                await uOw.LoanApplicationsReadRepository.GetPendingLoanApplicationsAsync(pageNumber, pageSize,
                    startDate, endDate);
            
            var getPendingLoanApplicationsResponses = loanApplications.Select(x => x.ToVm());

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new PaginatorVm<IEnumerable<GetPendingLoanApplicationsResponse>>(
                pageSize, pageNumber, totalPages, totalCount, getPendingLoanApplicationsResponses);
        }
    }
}