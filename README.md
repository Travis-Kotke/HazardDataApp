# HazardDataApp

.NET 10 MAUI Android and Windows app for the admin companion to the student safety app.
Home and Add Article are placeholders. Quick Stats shows summary news, recent
incidents, and a seven-day report trend.
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

UI references only BusinessLogic. Quick Stats currently uses `DemoReportService`
to show sample data, marked by a `DEMO DATA` badge. In `MauiProgram.cs`, remove
the demo `IReportService` override to use the database-backed implementation
registered by `AddHazardBusinessLogic()`. `IReportService` and `IReportRepository`
allow replacement implementations. When switching, also remove the `DEMO DATA`
badge from `QuickStatsPage.xaml` and delete `UI/Services/DemoReportService.cs`.

## Supabase setup

1. Copy `HazardDataApp.Database/SupabaseConfig.example.cs` to
   `HazardDataApp.Database/SupabaseConfig.cs` in the same folder.
2. Set `Url` and `PublishableKey` to the existing project's URL and public
   publishable (`sb_publishable_...`) or legacy anon key. The real file is ignored
   by Git. Never put a service_role or secret key in this app.
3. Without that file, the example is compiled so a clean checkout still builds.
   Quick Stats continues to show demo data until its demo service override is
   removed.
4. Use the existing `public.reports` schema. Counts include only rows visible to
   the current Supabase role through row level security (RLS). Authentication is
   outside this skeleton: if the existing policies require an authenticated admin,
   the team must connect its admin sign-in/session before protected data is visible.
   Keep existing protections; do not enable anonymous access to private reports.

The report timestamps are mapped as `DateTime` because the supplied schema says
`timestamp`; nullable floor/date_resolved are preserved. Severity and floor remain
strings matching the existing database values. No report write workflow is included.

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
After configuring Supabase and removing the demo override, it will use
permitted report reads instead. No live backend credentials are included.

## Proposed articles table

Review and run `sql/create_articles.sql` manually in the Supabase SQL editor.
The proposed fields are `id`, `title`, `body`, `author`, `date_published`, matching
`BusinessLogic/Models/Article.cs`. The SQL enables RLS and grants no client
policies. The team must define student read/admin write policies before adding
article persistence. This skeleton does not execute SQL or access articles.

The root `Platforms`, `Resources`, and `Properties` folders were already present;
copies under UI are used by the new project. The original files are preserved.

References: [Supabase C# client](https://www.nuget.org/packages/Supabase) and
[MAUI installation](https://learn.microsoft.com/en-us/dotnet/maui/get-started/installation?view=net-maui-10.0).
