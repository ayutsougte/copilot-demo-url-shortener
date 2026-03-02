# EF Core Migrations - Development Instructions

This project uses Entity Framework Core with SQLite. Migrations are checked into source under `ShortLinkApp.Api/Data/Migrations` and are applied automatically on application startup.

Local development workflow:

- To add a new migration:

  1. From the repository root run:
     ```
     dotnet ef migrations add YourMigrationName --project ShortLinkApp.Api --startup-project ShortLinkApp.Api
     ```

  2. Commit the generated files under `ShortLinkApp.Api/Data/Migrations`.

- To apply migrations to the database locally (if you prefer manual apply):

  ```
  dotnet ef database update --project ShortLinkApp.Api --startup-project ShortLinkApp.Api
  ```

Notes:
- The application currently calls `db.Database.Migrate()` during startup which will apply any pending migrations automatically. If you prefer to manage migrations manually in your environment, remove that call in `Program.cs`.
- Ensure the `Microsoft.EntityFrameworkCore.Design` package is available (already referenced in the API project) to use the `dotnet ef` tools.

