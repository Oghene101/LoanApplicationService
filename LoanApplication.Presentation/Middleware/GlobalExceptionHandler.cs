using LoanApplication.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplication.Presentation.Middleware;

internal sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occurred while processing request");

        var problemDetails = exception switch
        {
            ValidationException validationException => new ValidationProblemDetails
            {
                Type = $"https://httpstatuses.com/{StatusCodes.Status400BadRequest}",
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more validation errors occurred",
                Errors = validationException.Errors,
                Instance = httpContext.Request.Path
            },
            ApiException apiException => new ProblemDetails
            {
                Type = $"https://httpstatuses.com/{apiException.StatusCode}",
                Title = apiException.Title,
                Status = apiException.StatusCode,
                Detail = apiException.Message,
                Extensions = apiException.Extensions!,
                Instance = httpContext.Request.Path,
            },
            _ => new ProblemDetails
            {
                Type = $"https://httpstatuses.com/{StatusCodes.Status500InternalServerError}",
                Title = "Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An unexpected error occurred",
                Instance = httpContext.Request.Path,
            }
        };

        httpContext.Response.Clear();
        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        if (problemDetails is ValidationProblemDetails vpd)
        {
            await httpContext.Response.WriteAsJsonAsync(vpd, cancellationToken);
        }
        else
        {
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        }

        return true;
    }
}