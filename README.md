# HazardDataApp

.NET 10 MAUI Android skeleton for the admin companion to the student safety app.
Home and Add Article are placeholders. Quick Stats loads total, open and resolved counts.
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

UI references only BusinessLogic. Database registration is composed through
`AddHazardBusinessLogic()` in `MauiProgram.cs`. `IReportService` and
`IReportRepository` allow replacement implementations. The singleton connection
initializes one shared client asynchronously on its first read.

## Supabase setup

1. Copy `HazardDataApp.Database/SupabaseConfig.example.cs` to
   `HazardDataApp.Database/SupabaseConfig.cs` in the same folder.
2. Set `Url` and `PublishableKey` to the existing project's URL and public
   publishable (`sb_publishable_...`) or legacy anon key. The real file is ignored
   by Git. Never put a service_role or secret key in this app.
3. Without that file, the example is compiled so a clean checkout still builds;
   Quick Stats displays a setup message instead of querying placeholders.
4. Use the existing `public.reports` schema. Counts include only rows visible to
   the current Supabase role through row level security (RLS). Authentication is
   outside this skeleton: if the existing policies require an authenticated admin,
   the team must connect its admin sign-in/session before protected data is visible.
   Keep existing protections; do not enable anonymous access to private reports.

The report timestamps are mapped as `DateTime` because the supplied schema says
`timestamp`; nullable floor/date_resolved are preserved. Severity and floor remain
strings matching the existing database values. No report write workflow is included.

## Android build and run

Install the .NET 10 SDK, JDK 21, Android SDK (including the platform required by
your .NET Android workload), and an Android emulator. Install the MAUI workload:

```powershell
dotnet workload install maui-android
dotnet restore HazardDataApp.sln
dotnet build HazardDataApp.UI/HazardDataApp.UI.csproj -f net10.0-android
```

Start an Android emulator in Android Device Manager, then run:

```powershell
dotnet build HazardDataApp.UI/HazardDataApp.UI.csproj -t:Run -f net10.0-android
```

Verification: the Android Debug build completed with zero warnings and zero
errors, producing `com.hazarddataapp.admin-Signed.apk` under
`HazardDataApp.UI/bin/Debug/net10.0-android/`. Live Supabase reads and emulator
execution were not verified because backend credentials were not supplied.

If SDK discovery fails, supply `-p:AndroidSdkDirectory="C:\path\to\Android\Sdk"`
and `-p:JavaSdkDirectory="C:\path\to\jdk-21"`. Android API 24 is the minimum
supported runtime. Home is Shell's first declared tab. Navigate using the tabs,
or Shell routes `//main/home`, `//main/add-article`, `//main/quick-stats`.

With configured credentials and permitted report reads, open Quick Stats and
compare the counts to the table; Refresh repeats the read. A connection error is
shown as an error, not as zero reports. No live backend credentials are included.

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
