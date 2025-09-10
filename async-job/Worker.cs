using async_job.queue;

namespace async_job;

public class Worker(ITaskQueue queue, ILogger<Worker> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        return DequeueItemAsync(stoppingToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Worker stopping at: {time}", DateTimeOffset.Now);
        return base.StopAsync(cancellationToken);
    }

    private async Task DequeueItemAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var item = await queue.DequeueAsync(stoppingToken);
                await item(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Worker thread stopped");
                break;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Something went wrong");
            }
        }
    }
}