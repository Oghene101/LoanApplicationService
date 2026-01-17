using Asp.Versioning;
using LoanApplication.Presentation.Abstractions;
using LoanApplication.Presentation.Middleware;

namespace LoanApplication.Presentation.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddOpenApi("v1");
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