using Asp.Versioning;
using LoanApplication.Application.Common.Contracts;
using LoanApplication.Application.Extensions;
using LoanApplication.Presentation.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using LoanApp = LoanApplication.Application.Common.Contracts.LoanApplication;

namespace LoanApplication.Presentation.Endpoints;

internal sealed class LoanApplicationEndpoints : IEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = app.MapGroup("api/v{apiVersion:apiVersion}/loan-application")
            .WithTags("LoanApplication")
            .WithApiVersionSet(apiVersionSet);

        group.MapPost("", ApplyForLoanAsync)
            .WithName("ApplyForLoan")
            .WithSummary("Apply for a loan")
            .WithDescription("Creates a new loan application for the authenticated user.")
            .RequireAuthorization();
    }

    private static async Task<Results<Ok<ApiResponse>, BadRequest<ValidationProblemDetails>>> ApplyForLoanAsync(
        LoanApp.ApplyForLoanRequest request,
        ISender sender, CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        string result = await sender.Send(command, cancellationToken);
        var apiResponse = ApiResponse.Success(message: result);

        return TypedResults.Ok(apiResponse);
    }
}