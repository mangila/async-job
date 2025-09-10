# async-job

A minimal .NET 9 Web API that demonstrates how to run asynchronous/background jobs with a simple in-memory work queue while tracking job status in Redis.

You trigger a job via an HTTP endpoint, the job is executed by a background worker (HostedService), and the current status is stored in Redis so it can be polled later.

- Queue implementation: `ITaskQueue` with `DefaultTaskQueue` (channels under the hood)
- Background worker: `Worker` dequeues and executes jobs
- Status store: Redis via `IDistributedCache` (StackExchange.Redis)
- API: Minimal API endpoints to start a job and query status

Redis Stack is provided via Docker Compose, including RedisInsight for inspecting keys.

RedisInsight URL (once docker compose is up):
- http://localhost:8001

## How it works
1. POST `/start-job` returns a GUID immediately and writes status `PENDING` in Redis.
2. The background worker picks up the work item, sets status to `STARTING`, sleeps for ~30 seconds to simulate work, then sets status to `COMPLETE` in Redis.
3. GET `/status-job/{guid}` returns the current status from Redis (`PENDING`, `STARTING`, `COMPLETE`, or `null` if not found/expired).

Key files:
- `Program.cs` — DI setup, Redis cache registration, and API endpoints
- `Worker.cs` — background worker loop
- `queue/TaskQueue.cs` & `queue/DefaultTaskQueue.cs` — simple queue abstraction/implementation
- `appsettings.json` — Redis connection string

## Prerequisites
- .NET SDK 9.0+
- Docker (to run Redis Stack + RedisInsight)

## Run Redis (with RedisInsight)
From the project root folder (where `compose.yaml` is located):

- Start: `docker compose up -d`
- Stop: `docker compose down`

This will start Redis on port 6379 and RedisInsight on port 8001.
- Redis: `localhost:6379`
- RedisInsight: http://localhost:8001

In RedisInsight, add a database with the host `localhost` and port `6379` to browse keys written by the app.

## Configure the app
Default config (in `async-job/appsettings.json`):

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

If you run Redis elsewhere, update the `Redis` connection string accordingly or set the environment variable:
- `ConnectionStrings__Redis=<your-redis-endpoint>`

## Run the app
From the `async-job` project folder:

- `dotnet run`

The default HTTP URL from `launchSettings.json` is:
- http://localhost:5079

## Try it out
A ready-made HTTP file is provided: `async-job/async-job.http` (works in JetBrains Rider, VS Code REST Client, or similar tools).

Manual steps with curl (alternatively use the `.http` file):

1) Start a job
```
curl -X POST http://localhost:5079/start-job -H "Accept: application/json"
```
Response example:
```
"5c9f2f45-0aef-48c1-8c9b-2f4d7b5f5a61"
```

2) Check status
```
curl http://localhost:5079/status-job/5c9f2f45-0aef-48c1-8c9b-2f4d7b5f5a61
```
Possible values: `PENDING`, `STARTING`, `COMPLETE`, or empty if not found.