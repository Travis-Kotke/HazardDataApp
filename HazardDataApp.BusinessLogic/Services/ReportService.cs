using HazardDataApp.BusinessLogic.Models;
using HazardDataApp.Database.Repositories;

namespace HazardDataApp.BusinessLogic.Services;

// Business rules and calculations belong here, outside pages and database access.
internal sealed class ReportService(IReportRepository repository) : IReportService
{
    public async Task<ReportStatistics> GetStatisticsAsync()
    {
        var reports = await repository.GetAllAsync();
        var resolved = reports.Count(report => report.IsResolved);
        return new ReportStatistics(reports.Count, reports.Count - resolved, resolved);
    }
}
