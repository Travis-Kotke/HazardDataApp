using HazardDataApp.BusinessLogic.Models;
using HazardDataApp.Database.Repositories;
using HazardDataApp.Database.Models;

namespace HazardDataApp.BusinessLogic.Services;

// Business rules and calculations belong here, outside pages and database access.
internal sealed class ReportService(IReportRepository repository) : IReportService
{
    public async Task<IReadOnlyList<Report>> GetReportsAsync()
    {
        var reports = await repository.GetAllAsync();
        return reports.Select(MapReport).ToList();
    }

    public async Task<Report> SetFlaggedAsync(int id, bool isFlagged)
    {
        if (id <= 0) throw new ArgumentException("A valid report is required.", nameof(id));
        return MapReport(await repository.SetFlaggedAsync(id, isFlagged));
    }

    private static Report MapReport(ReportRecord report) => new(
            report.Id, report.Title, report.Description, report.Severity, report.Location,
            report.IsInside, report.Floor, report.ReportedBy, report.DateReported,
            report.IsResolved, report.DateResolved, report.ResolutionNotes,
            report.Published, report.IsFlagged);

    public async Task<ReportStatistics> GetStatisticsAsync()
    {
        var reports = await repository.GetAllAsync();
        var resolved = reports.Count(report => report.IsResolved);
        return new ReportStatistics(reports.Count, reports.Count - resolved, resolved);
    }
}
