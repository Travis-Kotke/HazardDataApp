namespace HazardDataApp.BusinessLogic.Models;

public sealed class ReportFilter
{
    public string Title { get; init; } = "";
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? Severity { get; init; }
    public bool? IsResolved { get; init; }
    public bool PublishedOnly { get; init; }

    public IReadOnlyList<Report> Apply(IEnumerable<Report> reports)
    {
        if (FromDate.HasValue && ToDate.HasValue && FromDate.Value.Date > ToDate.Value.Date)
            throw new ArgumentException("The start date must be on or before the end date.");

        var title = Title.Trim();
        return reports
            .Where(report => title.Length == 0 || report.Title.Contains(title, StringComparison.OrdinalIgnoreCase))
            .Where(report => !FromDate.HasValue || report.DateReported.Date >= FromDate.Value.Date)
            .Where(report => !ToDate.HasValue || report.DateReported.Date <= ToDate.Value.Date)
            .Where(report => Severity is null || report.Severity == Severity)
            .Where(report => !IsResolved.HasValue || report.IsResolved == IsResolved.Value)
            .Where(report => !PublishedOnly || report.Published)
            .OrderByDescending(report => report.DateReported)
            .ThenByDescending(report => report.Id)
            .ToList();
    }
}
