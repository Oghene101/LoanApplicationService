using LoanApplication.Application.Extensions;
using LoanApplication.Infrastructure.Extensions;
using LoanApplication.Infrastructure.Persistence;
using LoanApplication.Presentation.Extensions;
using LoanApplication.Presentation.Middleware;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddPresentation()
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapScalarApiReference(options =>
{
    options.WithTitle("Loan Applications API")
        .WithTheme(ScalarTheme.BluePlanet)
        .SortTagsAlphabetically()
        .SortOperationsByMethod()
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
        .AddPreferredSecuritySchemes("Bearer");
});


app.UseHttpsRedirection();
app.UseMiddleware<TimingMiddleware>();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapEndpoints();

await using var scope = app.Services.CreateAsyncScope();
await DbSeeder.SeedAsync(scope.ServiceProvider);

app.Run();