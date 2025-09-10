using async_job;
using async_job.queue;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITaskQueue, DefaultTaskQueue>();
builder.Services.AddHostedService<Worker>();
// Configure Redis
builder.Services.AddStackExchangeRedisCache(redisOptions =>
{
    var connectionString = builder.Configuration.GetConnectionString("Redis");
    redisOptions.Configuration = connectionString;
    redisOptions.InstanceName = "async-job:";
});

var app = builder.Build();

app.MapPost("/start-job",
    async (HttpContext ctx, ITaskQueue queue, IDistributedCache redis, ILogger<Program> logger) =>
    {
        var guid = Guid.NewGuid();
        redis.SetString(guid.ToString(), "PENDING");
        await queue.EnqueueAsync(async ct =>
        {
            await redis.SetStringAsync(guid.ToString(), "STARTING", ct);
            // simulate task execution
            await Task.Delay(TimeSpan.FromSeconds(30), ct);
            await redis.SetStringAsync(guid.ToString(), "COMPLETE", ct);
        });
        return guid;
    });

app.MapGet("/status-job/{guid}",
    (string guid, IDistributedCache redis, ILogger<Program> logger) => redis.GetString(guid));

app.Run();