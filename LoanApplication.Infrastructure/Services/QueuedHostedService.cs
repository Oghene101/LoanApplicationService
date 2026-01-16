using LoanApplication.Application.Common.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LoanApplication.Infrastructure.Services;

internal sealed class QueuedHostedService(
    IBackgroundTaskQueue taskQueue,
    IServiceProvider serviceProvider,
    ILogger<QueuedHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{QueuedHostedService} is starting.", nameof(QueuedHostedService));

        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await taskQueue.DequeueAsync(stoppingToken);
            try
            {
                using var scope = serviceProvider.CreateScope();
                var sp = scope.ServiceProvider;
                await workItem.Run(sp, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred executing {TaskName} in the background {QueuedHostedService}.",
                    workItem.TaskName, nameof(QueuedHostedService));
            }
        }
    }
}