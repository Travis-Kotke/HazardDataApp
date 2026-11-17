using HazardDataApp.BusinessLogic.Models;
using HazardDataApp.BusinessLogic.Services;

namespace HazardDataApp.UI.Services;

internal sealed class DemoReportService(IReportService reportService) : IReportService
{
    private static readonly QuickStatsData demoData = CreateDemoData();

    // Only statistics are demo data; report reads/writes retain the real implementation.
    public Task<IReadOnlyList<Report>> GetReportsAsync() => reportService.GetReportsAsync();

    public Task<Report> SetFlaggedAsync(int id, bool isFlagged) =>
        reportService.SetFlaggedAsync(id, isFlagged);

    public Task<ReportStatistics> GetStatisticsAsync() =>
        Task.FromResult(demoData.Statistics);

    public Task<QuickStatsData> GetQuickStatsAsync() =>
        Task.FromResult(demoData);

    private static QuickStatsData CreateDemoData()
    {
        var now = DateTime.Now;
        var today = now.Date;
        var dailyCounts = new[] { 1, 0, 2, 4, 1, 3, 2 }
            .Select((count, index) => new DailyReportCount(today.AddDays(index - 6), count))
            .ToArray();

        var recentReports = new[]
        {
            new RecentReport("Slip near Student Union entrance", "Student Union", "Moderate", now.AddHours(-2), false),
            new RecentReport("Smoke alarm in North Hall", "North Hall", "High", today.AddDays(-1).AddHours(20), false),
            new RecentReport("Trip hazard in Library lobby", "Library", "Low", today.AddDays(-2).AddHours(14), false),
            new RecentReport("Door sensor triggered at Science Center", "Science Center", "Low", today.AddDays(-3).AddHours(9), true),
            new RecentReport("Water leak near Arts Building", "Arts Building", "Moderate", today.AddDays(-5).AddHours(11), true)
        };

        var summaryNews = new[]
        {
            "Four reports were filed three days ago, the busiest day in the past week.",
            "Two of the five most recent reports have been marked resolved.",
            "Recent reports include incidents in campus entrances and common areas."
        };

        return new QuickStatsData(
            new ReportStatistics(Total: 18, Open: 11, Resolved: 7),
            summaryNews,
            recentReports,
            dailyCounts);
    }
}
