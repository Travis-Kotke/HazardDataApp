# HazardDataApp

.NET 10 MAUI Android and Windows admin companion to the student safety app.
Home lists and filters reports. Add Article provides an in-memory article editor/list.
Quick Stats shows demo summaries, recent incidents, and a seven-day trend.
See [FILES.md](FILES.md) for every created file, including copied platform files/assets.

```text
HazardDataApp.sln
  HazardDataApp.UI             pages, Shell navigation, startup/DI
          |
          v
  HazardDataApp.BusinessLogic  services, models, statistics/rules
          |
          v
  HazardDataApp.Database       repositories, table mappings, shared Supabase client
          |
          v
       Supabase
```

UI references only BusinessLogic. Home uses the database-backed `IReportService`
registered by `AddHazardBusinessLogic()`. Quick Stats alone uses `DemoReportService`
to show sample statistics, marked by a `DEMO DATA` badge. In `MauiProgram.cs`,
replace the `QuickStatsPage` factory registration with
`builder.Services.AddSingleton<QuickStatsPage>();` to use the real report service.
Then remove the demo service registration, `DEMO DATA` badge from
`QuickStatsPage.xaml`, and `UI/Services/DemoReportService.cs`. `IReportService` and
`IReportRepository` allow replacement implementations.

## Supabase setup

The shared `HazardDataApp.Database/SupabaseConfig.defaults.cs` contains this
project's public API URL and publishable key. Teammates can pull, build, and run
without editing a configuration file. These are public client settings; access
still depends on Supabase policies. Never use a service_role or secret key here.

For an optional local override (for example, a different development project):

1. Copy `HazardDataApp.Database/SupabaseConfig.example.cs` to
   `HazardDataApp.Database/SupabaseConfig.cs` in the same folder.
2. Replace the placeholders with your alternate URL and publishable/anon key.
   This local file is ignored by Git and takes precedence over the shared defaults.

Quick Stats continues to show demo data until its page registration is switched
to the real report service.

Use the existing `public.reports` schema. Counts include only rows visible to
   the current Supabase role through row level security (RLS). Authentication is
   outside this skeleton: if the existing policies require an authenticated admin,
   the team must connect its admin sign-in/session before protected data is visible.
   Keep existing protections; do not enable anonymous access to private reports.

The report timestamps are mapped as `DateTime` because the supplied schema says
`timestamp`; nullable floor/date_resolved are preserved. Severity and floor remain
strings matching the existing database values. Home supports flag/unflag updates;
resolution updates are still pending team decisions.

## Build and run

Install the .NET 10 SDK and MAUI workloads for the platforms you plan to build:

```powershell
dotnet restore HazardDataApp.sln
dotnet build HazardDataApp.UI/HazardDataApp.UI.csproj -f net10.0-windows10.0.19041.0
dotnet build HazardDataApp.UI/HazardDataApp.UI.csproj -f net10.0-android
```

The Android target requires the .NET Android workload, JDK, and Android SDK.
Run from Visual Studio by selecting `Windows Machine` or an Android emulator.

Home is Shell's first declared tab. Navigate using the tabs, or Shell routes
`//main/home`, `//main/add-article`, and `//main/quick-stats`.

Quick Stats currently shows sample reports and a generated seven-day trend.
After configuring Supabase and switching its page registration, it will use
permitted report reads instead. No live backend credentials are included.

## Homepage work (`feature/homepage`)

Windows desktop preview is also supported. Install the Windows workload and run
the unpackaged app (no MSIX signing needed):

```powershell
dotnet workload install maui-windows
dotnet build HazardDataApp.UI/HazardDataApp.UI.csproj -t:Run -f net10.0-windows10.0.19041.0
```

The homepage follows the team's table-and-filter sketch. It loads reports through
`IReportService`, supports incident-title search, optional inclusive date ranges,
severity, resolved status, and public-only filtering (`is_published`). Reports sort
newest first. Clear filters restores all loaded reports; Refresh reloads Supabase.
The filter panel moves above the horizontally scrollable table on small screens.

Flag/Unflag is wired to mark a suspicious report without resolving it, but its
buttons are disabled because the current schema has no flag column. Before
enabling and testing them, run `sql/add_report_flag.sql` to add
`is_flagged boolean not null default false`. The existing publication column is
`is_published boolean`.
Existing admin SELECT/UPDATE policies must allow the action. Failed writes leave
the displayed report unchanged; the app does not change policies or run SQL.

Type filtering is disabled pending confirmation of the new column and category
values. Resolve/Reopen buttons are disabled pending confirmation of how the team
wants `date_resolved` handled. Resolved-status filtering already works.

After integrating the latest main, Android and Windows builds pass with no
errors. Add Article has existing obsolete `DisplayAlert` warnings. Ten temporary
integration checks passed for real report mapping/filtering, inclusive dates,
statistics/trends, demo isolation, flag delegation, and invalid input handling.
Live reads/writes and screen testing still require backend setup/device testing.

## Proposed articles table

Review and run `sql/create_articles.sql` manually in the Supabase SQL editor.
The proposed fields are `id`, `title`, `body`, `author`, `date_published`, matching
`BusinessLogic/Models/Article.cs`. The SQL enables RLS and grants no client
policies. The team must define student read/admin write policies before adding
article persistence. Add Article currently maintains an in-memory list only;
it does not save articles to Supabase. The app does not execute SQL.

The root `Platforms`, `Resources`, and `Properties` folders were already present;
copies under UI are used by the new project. The original files are preserved.

References: [Supabase C# client](https://www.nuget.org/packages/Supabase) and
[MAUI installation](https://learn.microsoft.com/en-us/dotnet/maui/get-started/installation?view=net-maui-10.0).
