using LoanApplication.Application.Extensions;
using LoanApplication.Infrastructure.Extensions;
using LoanApplication.Infrastructure.Persistence;
using LoanApplication.Presentation.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddPresentation()
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseHsts();
app.UseHttpsRedirection();
app.MapOpenApi();

app.MapScalarApiReference(options =>
{
    options.WithTitle("Loan Applications API")
        .WithTheme(ScalarTheme.BluePlanet)
        .SortTagsAlphabetically()
        .SortOperationsByMethod()
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
        .AddPreferredSecuritySchemes("Bearer");

    options.AddDocument("v1", "Loan Applications API v1", "/openapi/v1.json");
    options.AddDocument("v2", "Loan Applications API v2", "/openapi/v2.json");
});

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
});

app.UseAuthentication();
app.UseAuthorization();
app.MapEndpoints();

await using var scope = app.Services.CreateAsyncScope();
await DbSeeder.SeedAsync(scope.ServiceProvider);

app.Run();