using LoanApplication.Application.Common.Contracts;
using LoanApplication.Application.Common.Filters;
using LoanApplication.Application.Extensions;
using LoanApplication.Domain.Constants;
using LoanApplication.Presentation.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplication.Presentation.Endpoints;

public class LoanApplicationAdminEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/loan-application-admin").WithTags("LoanApplicationAdmin");

        group.MapGet("get-pending-loan-applications", GetPendingLoanApplicationsAsync)
            .WithName("GetPendingLoanApplications")
            .WithSummary("Get pending loan applications")
            .WithDescription("Returns a paginated list of pending loan applications for review.")
            .RequireAuthorization(Roles.LoanAdmin);

        group.MapPost("review-loan-application", ReviewLoanApplicationAsync)
            .WithName("ReviewLoanApplication")
            .WithSummary("Review a loan application")
            .WithDescription("Approve or reject a loan application by LoanAdmin.")
            .RequireAuthorization(Roles.LoanAdmin);
    }

    private static async Task<Results<
            Ok<ApiResponse<PaginatorVm<IEnumerable<LoanApplicationAdmin.GetPendingLoanApplicationsResponse>>>>,
            BadRequest<ValidationProblemDetails>>>
        GetPendingLoanApplicationsAsync(
            [AsParameters] LoanApplicationAdmin.GetPendingLoanApplicationsRequest request,
            ISender sender, CancellationToken cancellationToken)
    {
        var query = request.ToQuery();
        var result = await sender.Send(query, cancellationToken);
        var apiResponse = ApiResponse.Success(result.Value);

        return TypedResults.Ok(apiResponse);
    }

    private static async Task<Results<Ok<ApiResponse>, BadRequest<ValidationProblemDetails>>>
        ReviewLoanApplicationAsync(
            LoanApplicationAdmin.ReviewLoanApplicationRequest request,
            ISender sender, CancellationToken cancellationToken)
    {
        var query = request.ToCommand();
        string result = await sender.Send(query, cancellationToken);
        var apiResponse = ApiResponse.Success(message: result);

        return TypedResults.Ok(apiResponse);
    }
}