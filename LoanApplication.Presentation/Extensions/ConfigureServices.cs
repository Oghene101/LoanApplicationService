using Asp.Versioning;
using LoanApplication.Presentation.Abstractions;
using LoanApplication.Presentation.Middleware;

namespace LoanApplication.Presentation.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        // todo: add bearer security scheme transformer 
        // Implementation at https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/customize-openapi?view=aspnetcore-10.0
        services.AddOpenApi("v1", options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info.Title = "Loan Applications API";
                document.Info.Version = "v1";

                return Task.CompletedTask;
            });
        });
        services.AddOpenApi("v2");

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}

public static class EndpointRegistrationExtensions
{
    public static void MapEndpoints(this IEndpointRouteBuilder app)
    {
        var endpointTypes = typeof(IEndpoints).Assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && typeof(IEndpoints).IsAssignableFrom(t));

        foreach (var type in endpointTypes)
        {
            var instance = (IEndpoints)Activator.CreateInstance(type)!;
            instance.MapEndpoints(app);
        }
    }
}