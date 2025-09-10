using System.Threading.Channels;

namespace async_job.queue;

public class DefaultTaskQueue(ILogger<DefaultTaskQueue> logger) : ITaskQueue
{
    private readonly ILogger<DefaultTaskQueue> _logger = logger;

    private readonly Channel<Func<CancellationToken, ValueTask>> _queue =
        Channel.CreateUnbounded<Func<CancellationToken, ValueTask>>(
            new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false,
                AllowSynchronousContinuations = true
            });

    public async ValueTask EnqueueAsync(Func<CancellationToken, ValueTask> item)
    {
        ArgumentNullException.ThrowIfNull(item);
        await _queue.Writer.WriteAsync(item);
    }

    public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken)
    {
        var item = await _queue.Reader.ReadAsync(cancellationToken);
        return item;
    }
}