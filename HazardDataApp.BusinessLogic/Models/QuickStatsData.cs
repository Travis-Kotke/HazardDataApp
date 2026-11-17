namespace HazardDataApp.BusinessLogic.Models;

public sealed record RecentReport(
    string Title,
    string Location,
    string Severity,
    DateTime DateReported,
    bool IsResolved);

public sealed record DailyReportCount(DateTime Date, int Count);

public sealed record QuickStatsData(
    ReportStatistics Statistics,
    IReadOnlyList<string> SummaryNews,
    IReadOnlyList<RecentReport> RecentReports,
    IReadOnlyList<DailyReportCount> DailyCounts);
