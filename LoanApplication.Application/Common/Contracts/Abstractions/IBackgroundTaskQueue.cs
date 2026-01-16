namespace LoanApplication.Application.Common.Contracts.Abstractions;

public interface IBackgroundTaskQueue
{
    ValueTask QueueBackgroundWorkItemAsync(
        (Func<IServiceProvider, CancellationToken, Task> Run, string TaskName) workItem,
        CancellationToken cancellationToken = default);

    Task<(Func<IServiceProvider, CancellationToken, Task> Run, string TaskName)> DequeueAsync(
        CancellationToken cancellationToken);
}