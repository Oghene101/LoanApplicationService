using LoanApplication.Presentation.Abstractions;
using LoanApplication.Presentation.Middleware;

namespace LoanApplication.Presentation.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddOpenApi();
        //services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}

public static class EndpointRegistrationExtensions
{
    public static void MapEndpoints(this IEndpointRouteBuilder app)
    {
        var endpointTypes = typeof(IEndpoint).Assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && typeof(IEndpoint).IsAssignableFrom(t));

        foreach (var type in endpointTypes)
        {
            var instance = (IEndpoint)Activator.CreateInstance(type)!;
            instance.MapEndpoints(app);
        }
    }
}