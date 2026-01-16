using LoanApplication.Application.Common.Contracts;
using LoanApplication.Application.Common.Contracts.Abstractions;
using LoanApplication.Application.Common.Filters;
using LoanApplication.Application.Extensions;
using MediatR;
using SerilogTimings;
using GetPendingLoanApplicationsResponse =
    LoanApplication.Application.Common.Contracts.LoanApplicationAdmin.GetPendingLoanApplicationsResponse;

namespace LoanApplication.Application.Features.LoanApplicationAdmin.Queries;

public static class GetPendingLoanApplications
{
    public sealed record Query(
        PaginationFilter PaginationFilter,
        DateRangeFilter DateRangeFilter)
        : IRequest<Result<PaginatorVm<IEnumerable<GetPendingLoanApplicationsResponse>>>>;

    public sealed class Handler(
        IAuthService auth,
        IUnitOfWork uOw) : IRequestHandler<Query, Result<PaginatorVm<IEnumerable<GetPendingLoanApplicationsResponse>>>>
    {
        private static readonly string HandlerName = typeof(Handler).GetOuterAndInnerName();

        public async
            Task<Result<PaginatorVm<IEnumerable<GetPendingLoanApplicationsResponse>>>> Handle(Query request,
                CancellationToken cancellationToken)
        {
            var email = auth.GetSignedInUserEmail();
            using var op = Operation.Begin("{HandlerName} with Email: {Email}", HandlerName,
                email);

            var pageNumber = request.PaginationFilter.PageNumber;
            var pageSize = request.PaginationFilter.PageSize;
            var startDate = request.DateRangeFilter.EffectiveStartDate;
            var endDate = request.DateRangeFilter.EffectiveEndDate;

            var (loanApplications, totalCount) =
                await uOw.LoanApplicationsReadRepository.GetPendingLoanApplicationsAsync(pageNumber, pageSize,
                    startDate, endDate);

            var getPendingLoanApplicationsResponses = loanApplications.Select(x => x.ToVm());
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            op.Complete();
            return new PaginatorVm<IEnumerable<GetPendingLoanApplicationsResponse>>(
                pageSize, pageNumber, totalPages, totalCount, getPendingLoanApplicationsResponses);
        }
    }
}