# elaf-dot-net

ASP.NET Core 8 Razor Pages tender marketplace (tenders, tender detail, company profiles, auth, settings, admin console, tender form), bilingual English/Arabic.

The pages read and write a PostgreSQL database through EF Core, with a schema that follows the tender platform ERD (multi-language content, RTL-ready), ASP.NET Core Identity for sign-in, and a service layer behind every page.

## Run locally

Requires .NET 8 and PostgreSQL 14+.

No credentials live in this repository. The connection string and the administrator seed come from [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets).

```bash
dotnet tool restore

dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=munaqasat;Username=postgres;Password=<your password>"
dotnet user-secrets set "Seed:AdministratorEmail" "you@example.com"
dotnet user-secrets set "Seed:AdministratorPassword" "<a password of your own>"

dotnet run
```

In Development, startup applies migrations and seeds languages (en, ar), roles and the administrator. `Seed:SampleData` (on in `appsettings.Development.json`) also loads the sample tenders, companies, requests and ratings the UI was designed with. Re-running is safe: every seed step is idempotent.

In hosting, pass the same values as environment variables: `ConnectionStrings__Default`, `Seed__AdministratorEmail`, `Seed__AdministratorPassword`.

## Database

The schema follows the ERD in [`docs/schema.dbml`](docs/schema.dbml) (paste it into dbdiagram.io). Highlights:

- snake_case tables and columns, text ids, `timestamptz` everywhere, money as `numeric(12,3)`
- i18n: one translation row per record and language (`tender_translations`, `company_translations`, ...). Reads use the fallback **requested language, then the record's default, then English**. The request language comes from `?lang=`, the `munaqasat_lang` cookie, then the user's `preferred_language`.
- Postgres enums `request_status` and `text_direction`
- CHECK constraints (bond 0-100, price >= 0, days > 0, ratings 1-5, ...), a partial unique index for the single default language, a partial index for unread notifications, GIN indexes on array columns and trigram indexes on titles
- triggers (migration `DatabaseTriggers`): `updated_at` on every table that has it, and the rating aggregate that keeps `companies.avg_*` / `number_of_ratings` current

Additions to the ERD that the frontend needs are marked "added" in the DBML: `tenders.budget_min/budget_max/status` and `companies.verification_status`.

Auth uses ASP.NET Core Identity (`identity_*` tables), hosted in the app itself. `user_profiles.identity_user_id` links a profile to its Identity user.

Add a migration after changing an entity:

```bash
dotnet dotnet-ef migrations add <Name> -o Infrastructure/Persistence/Migrations
```

## Project layout

- `Domain/` — entities and enums mirroring the ERD
- `Infrastructure/Persistence/` — `ApplicationDbContext`, one configuration per area, migrations, seeder and sample data
- `Infrastructure/DependencyInjection.cs` — `AddPersistence`, `AddApplicationIdentity`, `AddApplicationServices`
- `Services/` — tenders (search, match score, create, requests), companies, accounts, request language
- `Models/` — view models the Razor Pages render; services map entities to these
- `Pages/` — Razor Pages (Account, Tenders, Companies, Settings, Admin)
- `Data/Pricing.cs` — pricing tiers (not in the ERD)
- `wwwroot/` — design system styles and interactivity

## Access

| Page | Who |
| --- | --- |
| Home, tender list and details, company profiles | everyone |
| Submit a request (tender details) | signed-in company accounts |
| Settings | signed-in users |
| Post a tender | Client and Administrator roles |
| Admin console | Administrator role |

## Guidelines

Conventions for anyone changing this codebase.

**Schema**
- `docs/schema.dbml` is the source of truth. Change it first, then the entity in `Domain/`, its configuration in `Infrastructure/Persistence/Configurations/`, then add a migration.
- Mark anything the ERD does not have with `note: 'added'` in the DBML.
- Never edit a migration that has run on a shared database. Add a new one.
- Every new table gets text ids (`Ids.New()`), `timestamptz` columns, and `IHasTimestamps` if it has `updated_at` (also add it to the `set_updated_at` trigger list).
- User-facing text goes in a `*_translations` table, never a plain column. Read it with `Translations.Pick` so the language fallback stays the same everywhere.

**Code**
- Pages call services; pages never touch `ApplicationDbContext` directly.
- Services return the view models in `Models/`. Keep those classes as the contract with the UI: add properties, don't rename or remove them without updating the views.
- Services report expected failures with `OperationResult.Failure("message")`. Exceptions are for bugs.
- Read the signed-in user through `ICurrentUser`, never from cookies or query strings.
- Dates: store UTC, show Oman time (`SystemClock.OmanOffset`).

**Security**
- No secrets in the repository: user secrets locally, environment variables in hosting.
- Every POST has an antiforgery token. Razor forms add it automatically; `fetch` calls send it (see `adminSetStatus` in `site.js`).
- Anything that changes state is a POST, including sign out.
- Protect pages in `Program.cs` (`AuthorizeFolder` / `AuthorizePage`), and check ownership again in the service.

**UX**
- Show real data or nothing. No hard-coded counts, placeholder deltas or sample ids in views.
- No dead controls. A button that does nothing yet is removed until its feature exists.
- Every table and list has an empty state.
- Status pills use the shared classes (`status-open`, `status-closed`, `status-draft`, `status-pending`, `status-accepted`, `status-rejected`) and come from the view model (`Tender.Status`/`StatusClass`, `TenderRequestRow.StatusClass`), not from the view.
- Everything must work in Arabic (RTL). Use logical CSS (`text-align:start`, `margin-inline-*`) instead of left/right.
- Confirm actions with the toast (`showToast`) and show a readable error when a request fails.

## Not built yet

- Forgot password and email confirmation (needs an email sender)
- Two-factor authentication (the Settings toggle is a placeholder)
- Chat, notifications and billing (tables exist; pages are UI only)
- Admin editing and deleting of tenders and users

## Configuration

| Key | Where it belongs | Purpose |
| --- | --- | --- |
| `ConnectionStrings:Default` | user secrets / environment | PostgreSQL connection string. Required. |
| `Seed:RunOnStartup` | `appsettings.*.json` | Migrate and seed on startup. On in Development, off elsewhere. |
| `Seed:SampleData` | `appsettings.*.json` | Load the sample content. On in Development only. |
| `Seed:AdministratorEmail` | user secrets / environment | Administrator account to create if it does not exist. |
| `Seed:AdministratorPassword` | user secrets / environment | Password for that account. Skipped unless both are set. |
| `Seed:AdministratorName` | user secrets / environment | Display name. Optional. |

## Deployment

Targets ASP.NET Core 8, suitable for MonsterASP.NET hosting via `dotnet publish` or the hosting panel's Git/FTP deployment. Run migrations against the production database before switching traffic (`Seed:RunOnStartup` is off outside Development).
