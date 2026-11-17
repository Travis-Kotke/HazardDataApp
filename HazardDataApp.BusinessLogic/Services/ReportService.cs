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

    public async Task<QuickStatsData> GetQuickStatsAsync()
    {
        var reports = await repository.GetAllAsync();
        var resolved = reports.Count(report => report.IsResolved);
        var statistics = new ReportStatistics(reports.Count, reports.Count - resolved, resolved);
        var today = DateTime.Today;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        var firstDayOfPreviousMonth = firstDayOfMonth.AddMonths(-1);
        var lastSevenDaysStart = today.AddDays(-6);
        var news = new List<string>();

        var increasedLocation = reports
            .Where(report => !string.IsNullOrWhiteSpace(report.Location))
            .GroupBy(report => report.Location, StringComparer.OrdinalIgnoreCase)
            .Select(group => new
            {
                Location = group.Key,
                CurrentMonth = group.Count(report => report.DateReported >= firstDayOfMonth && report.DateReported < firstDayOfMonth.AddMonths(1)),
                PreviousMonth = group.Count(report => report.DateReported >= firstDayOfPreviousMonth && report.DateReported < firstDayOfMonth)
            })
            .Where(group => group.CurrentMonth > group.PreviousMonth)
            .OrderByDescending(group => group.CurrentMonth - group.PreviousMonth)
            .FirstOrDefault();
        if (increasedLocation is not null)
            news.Add($"{increasedLocation.Location} has an increase in accidents from last month.");

        var floorWithMultipleReports = reports
            .Where(report => !string.IsNullOrWhiteSpace(report.Floor) && !string.IsNullOrWhiteSpace(report.Location))
            .GroupBy(report => new { report.Floor, report.Location })
            .Select(group => new { group.Key.Floor, group.Key.Location, Count = group.Count() })
            .Where(group => group.Count > 1)
            .OrderByDescending(group => group.Count)
            .FirstOrDefault();
        if (floorWithMultipleReports is not null)
            news.Add($"Multiple reports on the {floorWithMultipleReports.Floor} floor of {floorWithMultipleReports.Location}.");

        var quietLocation = reports
            .Where(report => !string.IsNullOrWhiteSpace(report.Location))
            .GroupBy(report => report.Location, StringComparer.OrdinalIgnoreCase)
            .Where(group => !group.Any(report => report.DateReported.Date >= lastSevenDaysStart && report.DateReported.Date <= today))
            .OrderByDescending(group => group.Max(report => report.DateReported))
            .Select(group => group.Key)
            .FirstOrDefault();
        if (quietLocation is not null)
            news.Add($"No reports in {quietLocation} this week.");

        if (news.Count == 0)
            news.Add("No notable changes in report activity this week.");

        var recentReports = reports
            .OrderByDescending(report => report.DateReported)
            .Take(5)
            .Select(report => new RecentReport(
                report.Title,
                report.Location,
                report.Severity,
                report.DateReported,
                report.IsResolved))
            .ToArray();

        var firstDay = today.AddDays(-6);
        var dailyCounts = Enumerable.Range(0, 7)
            .Select(offset =>
            {
                var date = firstDay.AddDays(offset);
                var count = reports.Count(report => report.DateReported.Date == date);
                return new DailyReportCount(date, count);
            })
            .ToArray();

        return new QuickStatsData(statistics, news, recentReports, dailyCounts);
    }
}
