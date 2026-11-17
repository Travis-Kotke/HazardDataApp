using HazardDataApp.BusinessLogic;
using HazardDataApp.BusinessLogic.Services;
using HazardDataApp.UI.Services;
using HazardDataApp.UI.Views;

#if MAUI_DEVFLOW
using Microsoft.Maui.DevFlow.Agent;
#endif

namespace HazardDataApp.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
#if MAUI_DEVFLOW
        builder.AddMauiDevFlowAgent();
#endif
        // UI registers business services; Database and Supabase stay behind that boundary.
        builder.Services.AddHazardBusinessLogic();
        // DEMO ONLY: Remove this override to use the database-backed service.
        builder.Services.AddSingleton<IReportService, DemoReportService>();
        builder.Services.AddSingleton<HomePage>();
        builder.Services.AddSingleton<AddArticlePage>();
        builder.Services.AddSingleton<QuickStatsPage>();
        builder.Services.AddSingleton<AppShell>();
        return builder.Build();
    }
}
