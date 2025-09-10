namespace async_job.queue;

public interface ITaskQueue
{
    ValueTask EnqueueAsync(Func<CancellationToken, ValueTask> item);

    ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(
        CancellationToken cancellationToken);
}