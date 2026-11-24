using HazardDataApp.BusinessLogic.Models;
using HazardDataApp.Database.Repositories;
using HazardDataApp.Database.Models;
using System.Globalization;

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

    public async Task<QuickStatsData> GetQuickStatsAsync(int? numberOfDays = null)
    {
        var reports = await repository.GetAllAsync();
        var resolved = reports.Count(report => report.IsResolved);
        var statistics = new ReportStatistics(reports.Count, reports.Count - resolved, resolved);
        var today = DateTime.Today;
        var news = new List<string>();

        var activeLocations = reports
            .Where(report => !string.IsNullOrWhiteSpace(report.Location))
            .GroupBy(report => report.Location.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => new { Location = group.Key, Count = group.Count() })
            .OrderByDescending(group => group.Count)
            .ThenBy(group => group.Location, StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .ToArray();
        if (activeLocations.Length > 0)
        {
            var locations = FormatList(activeLocations.Select(location =>
                $"{location.Location} ({location.Count})").ToArray());
            news.Add($"Most active locations: {locations}.");
        }

        var hazardCategories = reports
            .Select(report => new
            {
                report.Location,
                Category = PickHazardCategory(report.Description, report.Title),
                report.DateReported
            })
            .Where(report => report.Category is not null)
            .GroupBy(report => report.Category!)
            .Select(group => new
            {
                Category = group.Key,
                Count = group.Count(),
                Locations = group
                    .Where(report => !string.IsNullOrWhiteSpace(report.Location))
                    .GroupBy(report => report.Location, StringComparer.OrdinalIgnoreCase)
                    .OrderByDescending(location => location.Count())
                    .Take(2)
                    .Select(location => location.Key)
                    .ToArray()
            })
            .OrderByDescending(group => group.Count)
            .ThenBy(group => group.Category, StringComparer.Ordinal)
            .FirstOrDefault();
        if (hazardCategories is not null)
        {
            var outsideLocation = hazardCategories.Locations.Any(location =>
                string.Equals(location.Trim(), "Outside", StringComparison.OrdinalIgnoreCase));
            var otherLocations = hazardCategories.Locations
                .Where(location => !string.Equals(location.Trim(), "Outside", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            var locationText = outsideLocation
                ? $" Most reports were outside{(otherLocations.Length == 0 ? "" : $" and at {FormatList(otherLocations)}")}."
                : hazardCategories.Locations.Length == 0
                    ? ""
                    : $" Most reports were at {FormatList(hazardCategories.Locations)}.";
            news.Add($"Most common hazard type: {hazardCategories.Category} ({hazardCategories.Count} reports).{locationText}");
        }
        else if (reports.Count > 0)
            news.Add("No common hazard type could be identified from report descriptions.");

        var datedReports = reports
            .Where(report => report.DateReported != default && report.DateReported.Date <= today)
            .ToArray();
        var busiestDate = datedReports
            .GroupBy(report => report.DateReported.Date)
            .Select(group => new { Date = group.Key, Count = group.Count() })
            .OrderByDescending(group => group.Count)
            .ThenByDescending(group => group.Date)
            .FirstOrDefault();
        var busiestWeekday = datedReports
            .GroupBy(report => report.DateReported.DayOfWeek)
            .Select(group => new { Day = group.Key, Count = group.Count() })
            .OrderByDescending(group => group.Count)
            .ThenBy(group => group.Day)
            .FirstOrDefault();
        if (busiestDate is not null)
        {
            var weekdaySummary = busiestWeekday is null
                ? ""
                : $" {busiestWeekday.Day}s are the busiest weekday ({busiestWeekday.Count} reports total).";
            news.Add(
                $"Most hazardous day: {busiestDate.Date.ToString("MMM. dd", CultureInfo.InvariantCulture)} " +
                $"({busiestDate.Count} reports).{weekdaySummary}");
        }

        if (news.Count == 0)
            news.Add("No accident reports are available to summarize yet.");

        var recentReports = reports
            .OrderByDescending(report => report.DateReported)
            .Take(5)
            .Select(report => new RecentReport(
                report.Id,
                report.Title,
                report.Description,
                report.Location,
                report.Severity,
                report.IsInside,
                report.Floor,
                report.ReportedBy,
                report.DateReported,
                report.IsResolved,
                report.DateResolved,
                report.ResolutionNotes))
            .ToArray();

        var firstDay = numberOfDays.HasValue
            ? today.AddDays(-(numberOfDays.Value - 1))
            : datedReports.Length > 0
                ? datedReports.Min(report => report.DateReported).Date
                : today;
        firstDay = firstDay > today ? today : firstDay;
        var dayCount = Math.Max(1, (today - firstDay).Days + 1);
        var dailyCounts = Enumerable.Range(0, dayCount)
            .Select(offset =>
            {
                var date = firstDay.AddDays(offset);
                var count = reports.Count(report => report.DateReported.Date == date);
                return new DailyReportCount(date, count);
            })
            .ToArray();

        return new QuickStatsData(statistics, news, recentReports, dailyCounts);
    }

    private static string? PickHazardCategory(string description, string title)
    {
        var text = $"{title} {description}";
        if (ContainsAny(text, "fall", "falling", "slip", "trip", "ice"))
            return "Slip and fall";
        if (ContainsAny(text, "fire", "smoke", "burn", "flame"))
            return "Fire and smoke";
        if (ContainsAny(text, "water", "leak", "flood", "pipe"))
            return "Water leak";
        if (ContainsAny(text, "electrical", "shock", "wire", "outlet", "power"))
            return "Electrical";
        if (ContainsAny(text, "door", "glass", "broken", "trip hazard"))
            return "Structural";
        if (ContainsAny(text, "chemical", "spill", "hazardous", "gas"))
            return "Chemical";
        if (ContainsAny(text, "vehicle", "collision", "traffic", "parking"))
            return "Vehicle";
        return null;
    }

    private static bool ContainsAny(string text, params string[] keywords)
    {
        return keywords.Any(keyword =>
            text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static string FormatList(IReadOnlyList<string> items) => items.Count switch
    {
        0 => string.Empty,
        1 => items[0],
        2 => $"{items[0]} and {items[1]}",
        _ => $"{string.Join(", ", items.Take(items.Count - 1))}, and {items[^1]}"
    };
}
