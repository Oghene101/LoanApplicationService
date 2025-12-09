using LoanApplication.Presentation.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LoanApplication.Presentation.Endpoints;

public class WeatherForecastModule : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/weather-forecast").WithTags("WeatherForecast");

        group.MapPost("", GetWeatherForecast)
            .WithName("GetWeatherForecast")
            .WithSummary("Gets the next 5 days of weather forecasts")
            .WithDescription("Returns a list of weather forecasts for demonstration purposes.");
    }


    private Results<Ok<WeatherForecast[]>, NotFound> GetWeatherForecast()
    {
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();

        return TypedResults.Ok(forecast);
    }
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}