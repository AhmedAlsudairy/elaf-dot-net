# elaf-dot-net

ASP.NET Core 8 Razor Pages tender marketplace (tenders, tender detail, company profiles, auth, settings, admin console, tender form), bilingual English/Arabic.

The UI is built out and still reads from `Data/MockData.cs`. The backend behind it — EF Core schema, ASP.NET Core Identity and the service layer — is in place and ready to be wired into the pages.

## Run locally

Requires .NET 8 and a PostgreSQL 14+ server.

No credentials live in this repository. The connection string and the administrator seed come from [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets), which are stored per-developer outside the working tree.

```bash
dotnet tool restore

dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=munaqasat;Username=postgres;Password=<your password>"
dotnet user-secrets set "Seed:AdministratorEmail" "you@example.com"
dotnet user-secrets set "Seed:AdministratorPassword" "<a password of your own>"

dotnet dotnet-ef database update
dotnet run
```

Then open the URL printed in the console (e.g. `http://localhost:5022`).

On startup in Development the app runs pending migrations and seeds reference data (roles, sectors, the 11 governorates and their wilayats, subscription plans) plus the administrator account. Set `Seed:RunOnStartup` to `false` to turn that off. The administrator is skipped unless both seed values are set.

In hosting, pass the same values as environment variables instead: `ConnectionStrings__Default`, `Seed__AdministratorEmail`, `Seed__AdministratorPassword`.

## Verifying the backend

The schema and seeding can be checked end to end today. The Razor Pages still read `Data/MockData.cs`, so the service layer is not yet reachable through the UI.

```bash
dotnet build
dotnet dotnet-ef migrations list
dotnet dotnet-ef database update
dotnet run
```

A successful run logs the applied migration and the seed counts. Confirm the result in the database:

```sql
\dt
SELECT COUNT(*) FROM "Governorates";          -- 11
SELECT COUNT(*) FROM "Wilayats";              -- 61
SELECT COUNT(*) FROM "Sectors";               -- 16
SELECT COUNT(*) FROM "SubscriptionPlans";     -- 3
SELECT "Name" FROM "Roles" ORDER BY "Name";   -- Administrator, CompanyMember, CompanyOwner, Individual
SELECT "Email" FROM "Users";                  -- the seeded administrator
```

Re-running `dotnet run` must not duplicate any of those rows: the seeder is idempotent.

To prove the schema can be rebuilt from scratch:

```bash
dotnet dotnet-ef database drop --force
dotnet dotnet-ef database update
```

## Project layout

- `Domain/` — entities and enums, free of persistence concerns
- `Infrastructure/Persistence/` — `ApplicationDbContext`, one `IEntityTypeConfiguration` per aggregate, migrations, reference-data seeder
- `Infrastructure/DependencyInjection.cs` — `AddPersistence`, `AddApplicationIdentity`, `AddApplicationServices`
- `Services/` — application services: tenders, companies, bids, reviews, match scoring
- `Pages/` — Razor Pages (one folder per feature area: Account, Tenders, Companies, Settings, Admin)
- `Pages/Shared/_Layout.cshtml` — single layout that switches between the marketing shell and the signed-in app shell
- `Models/`, `Data/MockData.cs` — view models and the sample data the pages still use
- `wwwroot/css/site.css`, `wwwroot/js/site.js` — design system styles and interactivity (tabs, toasts, filters)

## Database

PostgreSQL via Npgsql. Money is `numeric(18,3)` because the rial has three decimal places. Enums are stored as text so the values stay readable in SQL and reordering a case cannot reinterpret existing rows. `CreatedAt` and `UpdatedAt` are stamped centrally in `ApplicationDbContext.SaveChanges`.

Add a migration after changing an entity:

```bash
dotnet dotnet-ef migrations add <Name> -o Infrastructure/Persistence/Migrations
```

## Configuration

| Key | Where it belongs | Purpose |
| --- | --- | --- |
| `ConnectionStrings:Default` | user secrets / environment | PostgreSQL connection string. Required. |
| `Seed:RunOnStartup` | `appsettings.*.json` | Migrate and seed on startup. On in Development, off elsewhere. |
| `Seed:AdministratorEmail` | user secrets / environment | Administrator account to create if it does not exist. |
| `Seed:AdministratorPassword` | user secrets / environment | Password for that account. Seeding is skipped unless both are set. |
| `Seed:AdministratorName` | user secrets / environment | Display name. Optional. |

The committed `appsettings.json` and `appsettings.Development.json` hold no credentials and must stay that way. Anything secret goes in user secrets locally and environment variables in hosting.

## Deployment

Targets ASP.NET Core 8, suitable for MonsterASP.NET hosting via `dotnet publish` or the hosting panel's Git/FTP deployment.
