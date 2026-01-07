using System.Net;
using System.Security.Cryptography;
using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using CharityDonationsApp.Application.Common.Contracts.Abstractions.Repositories;
using LoanApplication.Application.Common.Contracts.Abstractions;
using LoanApplication.Application.Common.Contracts.Abstractions.Mailing;
using LoanApplication.Domain.Constants;
using LoanApplication.Domain.Entities;
using LoanApplication.Infrastructure.Configurations;
using LoanApplication.Infrastructure.Persistence;
using LoanApplication.Infrastructure.Persistence.DbContexts;
using LoanApplication.Infrastructure.Persistence.Repositories;
using LoanApplication.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Polly;

namespace LoanApplication.Infrastructure.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions();
        services.AddDbRegistration(config);
        services.AddJwtAuth(config);
        services.AddServices();
        services.AddHttpClientExtensions(config);
        return services;
    }

    private static void AddDbRegistration(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        services.AddSingleton<IDbConnectionFactory>(_ =>
            new SqlConnectionFactory(config.GetConnectionString("DefaultConnection")!));

        services.AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
    }

    private static void AddJwtAuth(this IServiceCollection services, IConfiguration config)
    {
        var jwtSettings = config.GetSection("Security:Jwt").Get<JwtSettings>()!;
        var rsa = RSA.Create();
        rsa.ImportFromPem(jwtSettings.PublicKey);

        services.AddAuthentication(opts =>
            {
                opts.DefaultScheme =
                    opts.DefaultAuthenticateScheme =
                        opts.DefaultChallengeScheme =
                            opts.DefaultSignInScheme =
                                opts.DefaultForbidScheme =
                                    opts.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opts => opts.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new RsaSecurityKey(rsa),
                ClockSkew = TimeSpan.Zero // no extra time beyond expiration
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(Roles.Admin, policy => policy.RequireRole(Roles.Admin))
            .AddPolicy(Roles.LoanAdmin, policy => policy.RequireRole(Roles.LoanAdmin));
    }

    private static void AddOptions(this IServiceCollection services)
    {
        services.AddOptions<AuthSettings>()
            .BindConfiguration(AuthSettings.Path)
            .ValidateOnStart();

        services.AddOptions<JwtSettings>()
            .BindConfiguration(JwtSettings.Path)
            .ValidateOnStart();

        services.AddOptions<ApiEndpoints>()
            .BindConfiguration(ApiEndpoints.Path)
            .ValidateOnStart();

        services.AddOptions<EncryptionSettings>()
            .BindConfiguration(EncryptionSettings.Path)
            .ValidateOnStart();

        services.AddOptions<EmailSettings>()
            .BindConfiguration(EmailSettings.Path)
            .ValidateOnStart();

        services.AddOptions<LoanTenureSettings>()
            .BindConfiguration(LoanTenureSettings.Path)
            .ValidateOnStart();
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddHostedService<QueuedHostedService>();
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddSingleton<IEncryptionProvider, AesEncryptionProvider>();
        services.AddSingleton<IEmailTemplates, EmailTemplates>();
        services.AddScoped<IUtilityService, UtilityService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IApiClient, ApiClient>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void AddHttpClientExtensions(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpClient();
    }

    private static IHttpClientBuilder ConfigureStandardPolicies(this IHttpClientBuilder builder,
        IServiceCollection services)
    {
        using var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<ApiClient>>();

        // Configurable values (would ideally come from config)
        var retryCount = 3;
        var breakDuration = TimeSpan.FromSeconds(30);
        var handledEventsBeforeBreaking = 5;
        var timeoutDuration = TimeSpan.FromSeconds(30);

        var retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.RequestTimeout || (int)msg.StatusCode >= 500)
            .WaitAndRetryAsync(
                retryCount,
                attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                (outcome, timespan, retryAttempt, context) =>
                {
                    logger.LogWarning(
                        $"Retry {retryAttempt} after {timespan.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                });

        var circuitBreakerPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.RequestTimeout || (int)msg.StatusCode >= 500)
            .CircuitBreakerAsync(
                handledEventsBeforeBreaking,
                breakDuration,
                (outcome, breakDelay) =>
                {
                    logger.LogError(
                        $"Circuit broken! Breaking for {breakDelay.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                },
                () => logger.LogInformation("Circuit reset."),
                () => logger.LogInformation("Circuit is half-open, next call is a trial."));

        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(timeoutDuration);

        return builder
            .AddPolicyHandler(retryPolicy)
            .AddPolicyHandler(circuitBreakerPolicy)
            .AddPolicyHandler(timeoutPolicy)
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                MaxConnectionsPerServer = 10
            });
    }
}