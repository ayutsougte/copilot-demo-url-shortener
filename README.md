# ShortLink (copilot-demo-url-shortener)

ShortLink — a branded, internal URL shortening service with optional vanity aliases, expirations, and per-click analytics. Built with a Blazor dashboard, a C# backend, and SQLite (EF Core).

## Overview
ShortLink is a lightweight self-hosted URL shortener designed for marketing teams who need clean, branded links and measurable click data. It supports generated short codes, custom (vanity) aliases, expirations for time-limited promotions, and full click event tracking with analytics suitable for a simple dashboard.

## Key features
- Create shortened links with either generated short codes or custom aliases (vanity URLs).
- Optional expiration dates for time-limited links.
- Per-click tracking: each redirect records a `ClickEvent` with timestamp (and extensible metadata such as referrer).
- Blazor dashboard to create, list, manage links and view analytics.
- Simple REST API for link management and analytics.
- SQLite database using EF Core for lightweight deployment and portability.

## Tech stack
- Frontend: Blazor (Server or WebAssembly + hosted API)
- Backend: C# (.NET) Web API
- Database: SQLite with Entity Framework Core
- Optional: SignalR for real-time updates, background `IHostedService` for expiry processing

## Data model (summary)
- `Links` table
  - `Id`, `ShortCode`, `OriginalUrl`, `CustomAlias`, `CreatedAt`, `ExpiresAt`, `IsActive`
  - Indexes on `ShortCode` and `CustomAlias` for fast lookup
- `ClickEvents` table
  - `Id`, `LinkId` (FK), `ClickedAt`, (optional metadata like `Referrer`)
  - Cascade-delete or FK constraint to `Links`

## HTTP API (core endpoints)
- `GET /{shortCode}` — redirect endpoint: resolves short code, checks expiration/active state, records a click event, and issues 301/302 or returns 404/410.
- `POST /api/links` — create a new short link (accepts `originalUrl`, optional `customAlias`, optional `expiresAt`).
- `GET /api/links` — list links (with click counts).
- `GET /api/links/{id}` — get link details.
- `PUT /api/links/{id}` — update a link.
- `DELETE /api/links/{id}` — delete a link.
- `GET /api/links/{id}/analytics` — return total clicks and a time-series breakdown (clicks per day) for charting.

## Quick start (developer)
Prerequisites: __.NET SDK__ (compatible LTS), Git, optional `dotnet-ef` tool.

1. Clone
   - `git clone https://github.com/ayutsougte/copilot-demo-url-shortener.git`
2. Restore & build
   - `dotnet restore`
   - `dotnet build`
3. Apply migrations (if present)
   - Install tooling if needed: `dotnet tool install --global dotnet-ef`
   - `dotnet ef database update` (or use the project's migration/apply command)
   - Note: the app can create the SQLite DB on first run when migrations are applied at startup.
4. Run
   - `dotnet run --project <backend-project-path>`
   - Open the Blazor dashboard at the configured URL (see appsettings)
5. Example create request
   - POST `/api/links` with JSON:
     ```json
     {
       "originalUrl": "https://example.com/very/long/url",
       "customAlias": "summer-sale",
       "expiresAt": "2026-08-01T00:00:00Z"
     }
     ```

## Validation & behavior
- URL validation ensures submitted URLs are well-formed.
- Custom aliases are limited to URL-safe characters and must not collide with reserved routes.
- Expiration dates must be in the future; expired links return 410 (or a friendly expired page).
- Click recording should be atomic with the redirect to avoid lost events.

## Development notes
- Use EF Core migrations for schema changes; apply them during CI or on startup.
- Background expiry: consider `IHostedService` to mark expired links inactive (or validate expiry at redirect time).
- Analytics: backend should return aggregated counts and per-day time series for frontend charts.
- Tests: add integration/E2E tests that cover create → resolve → redirect → verify click recorded.

## Roadmap & issues
See `ISSUES.md` for the initial backlog: database schema, link management, click tracking, analytics, dashboard components, tests, and performance targets.

## Contributing
Branch from `main`, open PRs with descriptive titles, and include tests for new behaviors. Refer to `ISSUES.md` for planned tasks and priorities.

## License
Add a `LICENSE` file to the repository and reference it here.

## Contact / Support
Create issues in this repository for bugs and feature requests.

ShortLink — a branded, internal URL shortening service with optional vanity aliases, expirations, and per-click analytics. Built with a Blazor dashboard, a C# backend, and SQLite (EF Core).

## Overview
ShortLink is a lightweight self-hosted URL shortener designed for marketing teams who need clean, branded links and measurable click data. It supports generated short codes, custom (vanity) aliases, expirations for time-limited promotions, and full click event tracking with analytics suitable for a simple dashboard.

## Key features
- Create shortened links with either generated short codes or custom aliases (vanity URLs).
- Optional expiration dates for time-limited links.
- Per-click tracking: each redirect records a `ClickEvent` with timestamp (and extensible metadata such as referrer).
- Blazor dashboard to create, list, manage links and view analytics.
- Simple REST API for link management and analytics.
- SQLite database using EF Core for lightweight deployment and portability.

## Tech stack
- Frontend: Blazor (Server or WebAssembly + hosted API)
- Backend: C# (.NET) Web API
- Database: SQLite with Entity Framework Core
- Optional: SignalR for real-time updates, background `IHostedService` for expiry processing

## Data model (summary)
- `Links` table
  - `Id`, `ShortCode`, `OriginalUrl`, `CustomAlias`, `CreatedAt`, `ExpiresAt`, `IsActive`
  - Indexes on `ShortCode` and `CustomAlias` for fast lookup
- `ClickEvents` table
  - `Id`, `LinkId` (FK), `ClickedAt`, (optional metadata like `Referrer`)
  - Cascade-delete or FK constraint to `Links`

## HTTP API (core endpoints)
- `GET /{shortCode}` — redirect endpoint: resolves short code, checks expiration/active state, records a click event, and issues 301/302 or returns 404/410.
- `POST /api/links` — create a new short link (accepts `originalUrl`, optional `customAlias`, optional `expiresAt`).
- `GET /api/links` — list links (with click counts).
- `GET /api/links/{id}` — get link details.
- `PUT /api/links/{id}` — update a link.
- `DELETE /api/links/{id}` — delete a link.
- `GET /api/links/{id}/analytics` — return total clicks and a time-series breakdown (clicks per day) for charting.

## Quick start (developer)
Prerequisites: __.NET SDK__ (compatible LTS), Git, optional `dotnet-ef` tool.

1. Clone
   - `git clone https://github.com/ayutsougte/copilot-demo-url-shortener.git`
2. Restore & build
   - `dotnet restore`
   - `dotnet build`
3. Apply migrations (if present)
   - Install tooling if needed: `doten