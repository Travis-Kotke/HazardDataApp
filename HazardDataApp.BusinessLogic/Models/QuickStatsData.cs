namespace HazardDataApp.BusinessLogic.Models;

public sealed record RecentReport(
    int Id,
    string Title,
    string Description,
    string Location,
    string Severity,
    bool IsInside,
    string? Floor,
    string ReportedBy,
    DateTime DateReported,
    bool IsResolved,
    DateTime? DateResolved,
    string ResolutionNotes);

public sealed record DailyReportCount(DateTime Date, int Count);

public sealed record QuickStatsData(
    ReportStatistics Statistics,
    IReadOnlyList<string> SummaryNews,
    IReadOnlyList<RecentReport> RecentReports,
    IReadOnlyList<DailyReportCount> DailyCounts);
