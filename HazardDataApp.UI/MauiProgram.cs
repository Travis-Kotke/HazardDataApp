using HazardDataApp.BusinessLogic;
using HazardDataApp.BusinessLogic.Services;
using HazardDataApp.UI.Services;
using HazardDataApp.UI.Views;

namespace HazardDataApp.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
        // UI registers business services; Database and Supabase stay behind that boundary.
        builder.Services.AddHazardBusinessLogic();
        // Keep the Quick Stats demo local to that page; Home uses the real service.
        builder.Services.AddSingleton<DemoReportService>();
        builder.Services.AddSingleton<HomePage>();
        builder.Services.AddSingleton<AddArticlePage>();
        builder.Services.AddSingleton<QuickStatsPage>(services =>
            new QuickStatsPage(services.GetRequiredService<DemoReportService>()));
        builder.Services.AddSingleton<AppShell>();
        return builder.Build();
    }
}
