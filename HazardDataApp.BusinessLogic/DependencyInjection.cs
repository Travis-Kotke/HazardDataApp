using HazardDataApp.BusinessLogic.Services;
using HazardDataApp.Database;
using Microsoft.Extensions.DependencyInjection;

namespace HazardDataApp.BusinessLogic;

public static class DependencyInjection
{
    public static IServiceCollection AddHazardBusinessLogic(this IServiceCollection services)
    {
        services.AddHazardDatabase();
        services.AddSingleton<IReportService, ReportService>();
        return services;
    }
}
