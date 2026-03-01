# CopilotDemo URL Shortener – src

This directory contains the .NET solution with three projects:

| Project | Type | Description |
|---------|------|-------------|
| `CopilotDemo.Shared` | Class Library | Shared DTOs (`ShortLink`) |
| `CopilotDemo.Server` | ASP.NET Core Minimal API | Backend API running on https://localhost:5001 |
| `CopilotDemo.Client` | Blazor WebAssembly | Frontend app that calls the server |

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Build

```bash
cd src
dotnet build
```

## Run the Server

```bash
cd src/CopilotDemo.Server
dotnet run --launch-profile https
```

The server starts at **https://localhost:5001** (HTTP on http://localhost:5000).  
Swagger UI is available at https://localhost:5001/swagger in Development mode.

### Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/health` | Returns `{ "status": "ok" }` |
| POST | `/api/links` | Create a short link |
| GET | `/api/links/{key}` | Retrieve a short link by key |

## Run the Client

```bash
cd src/CopilotDemo.Client
dotnet run
# or for hot reload:
dotnet watch
```

The Blazor WebAssembly app runs on **https://localhost:7288** by default and expects the server to be running at **https://localhost:5001**.

## Notes

- The server uses an in-memory store; data is lost on restart.
- CORS is configured to allow requests from `https://localhost:7288`.
- Start the server before the client so API calls succeed.
