# elaf-dot-net

ASP.NET Core 8 Razor Pages UI for a bilingual tender marketplace (tenders, tender detail, company profiles, auth, settings, admin console, tender form). UI-only prototype backed by in-memory mock data — no database yet.

## Run locally

```bash
dotnet restore
dotnet run
```

Then open the URL printed in the console (e.g. `http://localhost:5022`).

## Project layout

- `Pages/` — Razor Pages (one folder per feature area: Account, Tenders, Companies, Settings, Admin)
- `Pages/Shared/_Layout.cshtml` — single layout that switches between the marketing shell and the signed-in app shell
- `Models/`, `Data/MockData.cs` — sample domain data
- `wwwroot/css/site.css`, `wwwroot/js/site.js` — design system styles and interactivity (tabs, toasts, filters)

## Deployment

Targets ASP.NET Core 8, suitable for MonsterASP.NET hosting via `dotnet publish` or the hosting panel's Git/FTP deployment.
