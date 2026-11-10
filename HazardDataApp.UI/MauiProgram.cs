using HazardDataApp.BusinessLogic;
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
        builder.Services.AddSingleton<HomePage>();
        builder.Services.AddSingleton<AddArticlePage>();
        builder.Services.AddSingleton<QuickStatsPage>();
        builder.Services.AddSingleton<AppShell>();
        return builder.Build();
    }
}
