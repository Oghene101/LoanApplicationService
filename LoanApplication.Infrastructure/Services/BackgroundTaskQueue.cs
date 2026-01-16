using System.Threading.Channels;
using LoanApplication.Application.Common.Contracts.Abstractions;

namespace LoanApplication.Infrastructure.Services;

internal sealed class BackgroundTaskQueue(int capacity = 100, int timeoutMs = 500) : IBackgroundTaskQueue
{
    private readonly Channel<(Func<IServiceProvider, CancellationToken, Task> Run, string TaskName)> _queue =
        Channel.CreateBounded<(Func<IServiceProvider, CancellationToken, Task> Run, string TaskName)>(capacity);

    public async ValueTask QueueBackgroundWorkItemAsync(
        (Func<IServiceProvider, CancellationToken, Task> Run, string TaskName) workItem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeoutMs);
        try
        {
            await _queue.Writer.WriteAsync(workItem, cts.Token);
        }
        catch (OperationCanceledException)
        {
            throw new InvalidOperationException(
                $"Background task queue is full (capacity {capacity}). Timed out after {timeoutMs}ms.");
        }
    }

    public async Task<(Func<IServiceProvider, CancellationToken, Task> Run, string TaskName)> DequeueAsync(
        CancellationToken cancellationToken)
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);
        return workItem;
    }
}