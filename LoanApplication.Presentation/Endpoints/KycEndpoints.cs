using LoanApplication.Application.Common.Contracts;
using LoanApplication.Application.Extensions;
using LoanApplication.Presentation.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplication.Presentation.Endpoints;

public class KycEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/kyc").WithTags("Kyc");

        group.MapPost("add-bvn", AddBvnAsync)
            .WithName("AddBvn")
            .WithSummary("Add BVN to user profile")
            .WithDescription("Associates a BVN with the authenticated user's account.")
            .RequireAuthorization();

        group.MapPost("add-nin", AddNinAsync)
            .WithName("AddNin")
            .WithSummary("Add NIN to user profile")
            .WithDescription("Associates a NIN with the authenticated user's account.")
            .RequireAuthorization();

        group.MapPost("add-address", AddAddressAsync)
            .WithName("AddAddress")
            .WithSummary("Add Address to user profile")
            .WithDescription("Associates an address with the authenticated user's account.")
            .RequireAuthorization();
    }

    private static async Task<Results<Ok<ApiResponse>, BadRequest<ValidationProblemDetails>>> AddBvnAsync(
        Kyc.AddBvnRequest request,
        ISender sender, CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        string result = await sender.Send(command, cancellationToken);
        var apiResponse = ApiResponse.Success(message: result);

        return TypedResults.Ok(apiResponse);
    }

    private static async Task<Results<Ok<ApiResponse>, BadRequest<ValidationProblemDetails>>> AddNinAsync(
        Kyc.AddNinRequest request,
        ISender sender, CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        string result = await sender.Send(command, cancellationToken);
        var apiResponse = ApiResponse.Success(message: result);

        return TypedResults.Ok(apiResponse);
    }

    private static async Task<Results<Ok<ApiResponse>, BadRequest<ValidationProblemDetails>>> AddAddressAsync(
        Kyc.AddAddressRequest request,
        ISender sender, CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        string result = await sender.Send(command, cancellationToken);
        var apiResponse = ApiResponse.Success(message: result);

        return TypedResults.Ok(apiResponse);
    }
}