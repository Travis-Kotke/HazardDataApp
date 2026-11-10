using HazardDataApp.Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HazardDataApp.Database;

public static class DependencyInjection
{
    public static IServiceCollection AddHazardDatabase(this IServiceCollection services)
    {
        services.AddSingleton<SupabaseConnection>();
        services.AddSingleton<IReportRepository, ReportRepository>();
        return services;
    }
}
