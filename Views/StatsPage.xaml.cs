using System.Collections.ObjectModel;
using HazardDataApp.BusinessLogic.Models;
using HazardDataApp.BusinessLogic.Services;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Media;

namespace HazardDataApp.UI.Views;

public sealed record RecentReportRow(
    string Title,
    string RelativeTime,
    Color BackgroundColor,
    RecentReport? Report);

public partial class StatsPage : ContentPage
{
    private readonly IReportService reportService;
    private readonly ObservableCollection<RecentReportRow> recentReports = new();
    private bool loading;
    private bool exporting;
    private bool ready;
    private string selectedTimeSpan = "All Time";
    private IReadOnlyList<DailyReportCount> allTimeDailyCounts = Array.Empty<DailyReportCount>();
    private readonly IReadOnlyDictionary<string, int?> timeSpanOptions =
        new Dictionary<string, int?>
        {
            ["All Time"] = null,
            ["Past 3 Days"] = 3,
            ["This Week"] = 7,
            ["2 Weeks"] = 14,
            ["1 Month"] = 30,
            ["3 Months"] = 90,
            ["6 Months"] = 180,
            ["1 Year"] = 365
        };

    public StatsPage(IReportService reportService)
    {
        InitializeComponent();
        this.reportService = reportService;
        RecentReportsList.ItemsSource = recentReports;
        TrendChart.Drawable = new ReportTrendDrawable();
        TrendChart.StartInteraction += OnTrendChartStartInteraction;
        ChartOverlay.SizeChanged += OnChartOverlaySizeChanged;
        foreach (var option in timeSpanOptions.Keys)
        {
            var button = new Button
            {
                Text = option,
                CommandParameter = option,
                BackgroundColor = Colors.Transparent,
                TextColor = Color.FromArgb("#172B4D"),
                FontSize = 14,
                Padding = new Thickness(10, 7),
                HorizontalOptions = LayoutOptions.Fill
            };
            button.Clicked += OnTimeSpanOptionClicked;
            TimeSpanOptionsLayout.Children.Add(button);
        }
        ready = true;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (loading) return;
        loading = true;
        StatusLabel.IsVisible = false;

        try
        {
            var data = await reportService.GetQuickStatsAsync();
            ShowStats(data);
        }
        catch (InvalidOperationException ex)
        {
            ShowError(ex.Message);
        }
        catch (Exception)
        {
            ShowError("Could not load reports. Check your connection, Supabase settings, and reports read policy.");
        }
        finally
        {
            loading = false;
        }
    }

    private void OnTimeSpanToggleTapped(object? sender, TappedEventArgs e)
    {
        if (!ready)
            return;

        if (!TimeSpanOptionsPanel.IsVisible)
            PositionTimeSpanOptionsPanel();
        TimeSpanOptionsPanel.IsVisible = !TimeSpanOptionsPanel.IsVisible;
        TimeSpanChevron.Text = TimeSpanOptionsPanel.IsVisible ? "⌃" : "⌄";
    }

    private void OnChartOverlaySizeChanged(object? sender, EventArgs e)
    {
        if (TimeSpanOptionsPanel.IsVisible)
            PositionTimeSpanOptionsPanel();
    }

    private void PositionTimeSpanOptionsPanel()
    {
        const double menuWidth = 160;
        const double menuHeight = 228;
        var left = Math.Max(0, ChartOverlay.Width - menuWidth);
        AbsoluteLayout.SetLayoutBounds(
            TimeSpanOptionsPanel,
            new Rect(left, 42, menuWidth, menuHeight));
    }

    private void OnTimeSpanOptionClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: string option } || !timeSpanOptions.ContainsKey(option))
            return;

        selectedTimeSpan = option;
        SelectedTimeSpanLabel.Text = option;
        TimeSpanOptionsPanel.IsVisible = false;
        TimeSpanChevron.Text = "⌄";
        RenderSelectedTimeSpan();
    }

    private async void OnTrendChartStartInteraction(object? sender, TouchEventArgs e)
    {
        if (!e.Touches.Any())
            return;
        if (TrendChart.Drawable is not ReportTrendDrawable drawable)
            return;

        var touch = e.Touches[0];
        var bounds = new RectF(0, 0, (float)TrendChart.Width, (float)TrendChart.Height);
        var reportCount = drawable.FindVertexAt(touch, bounds);
        if (reportCount is null)
            return;

        await DisplayAlertAsync(
            reportCount.Date.ToString("MMM. dd, yyyy"),
            $"Hazards reported: {reportCount.Count}",
            "Close");
    }

    private async void OnRecentAccidentSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not RecentReportRow { Report: not null } row)
            return;

        var report = row.Report;
        var location = string.IsNullOrWhiteSpace(report.Location) ? "Not specified" : report.Location;
        var floor = string.IsNullOrWhiteSpace(report.Floor) ? "Not specified" : report.Floor;
        var description = string.IsNullOrWhiteSpace(report.Description) ? "No description provided." : report.Description;
        var reportedBy = string.IsNullOrWhiteSpace(report.ReportedBy) ? "Unknown" : report.ReportedBy;
        var resolution = string.IsNullOrWhiteSpace(report.ResolutionNotes)
            ? "No resolution notes."
            : report.ResolutionNotes;
        var details =
            $"Description: {description}\n\n" +
            $"Location: {location}{(report.IsInside ? " (Inside)" : "")}\n" +
            $"Floor: {floor}\n" +
            $"Severity: {(string.IsNullOrWhiteSpace(report.Severity) ? "Not specified" : report.Severity)}\n" +
            $"Reported: {report.DateReported:f}\n" +
            $"Reported by: {reportedBy}\n" +
            $"Status: {(report.IsResolved ? "Resolved" : "Unresolved")}\n" +
            $"Resolution: {resolution}";

        await DisplayAlertAsync(
            string.IsNullOrWhiteSpace(report.Title) ? "Accident Details" : report.Title,
            details,
            "Close");

        if (sender is CollectionView collection)
            collection.SelectedItem = null;
    }

    private void ShowStats(QuickStatsData data)
    {
        SummaryNewsLayout.Children.Clear();
        foreach (var item in data.SummaryNews)
        {
            SummaryNewsLayout.Children.Add(new Border
            {
                BackgroundColor = Color.FromArgb("#F3F7FB"),
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                Padding = new Thickness(12, 10),
                Content = new Label
                {
                    Text = $"•   {item}",
                    FontSize = 15,
                    TextColor = Color.FromArgb("#26364A"),
                    LineBreakMode = LineBreakMode.WordWrap
                }
            });
        }

        recentReports.Clear();
        for (var index = 0; index < Math.Min(6, data.RecentReports.Count); index++)
        {
            var report = data.RecentReports[index];
            recentReports.Add(new RecentReportRow(
                string.IsNullOrWhiteSpace(report.Title) ? "Untitled report" : report.Title,
                FormatRelativeTime(report.DateReported),
                index % 2 == 0 ? Color.FromArgb("#F3F7FB") : Color.FromArgb("#FAFBFD"),
                report));
        }

        if (recentReports.Count == 0)
            recentReports.Add(new RecentReportRow("No accidents reported.", "", Color.FromArgb("#F3F7FB"), null));

        allTimeDailyCounts = data.DailyCounts;
        RenderSelectedTimeSpan();
    }

    private void RenderSelectedTimeSpan()
    {
        var days = timeSpanOptions[selectedTimeSpan];
        var counts = days.HasValue
            ? allTimeDailyCounts
                .Where(day => day.Date.Date >= DateTime.Today.AddDays(-(days.Value - 1)) &&
                              day.Date.Date <= DateTime.Today)
                .ToArray()
            : allTimeDailyCounts;
        ((ReportTrendDrawable)TrendChart.Drawable!).SetData(counts);
        TrendChart.Invalidate();
    }

    private void ShowError(string message)
    {
        StatusLabel.Text = message;
        StatusLabel.IsVisible = true;
    }

    private static string FormatRelativeTime(DateTime dateReported)
    {
        if (dateReported == default)
            return "";

        var elapsed = DateTime.Now - dateReported;
        if (elapsed.TotalMinutes < 1)
            return "Just now";
        if (elapsed.TotalHours < 1)
            return $"{Math.Max(1, (int)elapsed.TotalMinutes)} min ago";
        if (elapsed.TotalHours < 24)
            return $"{(int)elapsed.TotalHours} hr ago";

        var days = Math.Max(1, (DateTime.Today - dateReported.Date).Days);
        if (days == 1)
            return "Yesterday";
        if (days < 7)
            return $"{days} days ago";
        if (days < 30)
        {
            var weeks = days / 7;
            return $"{weeks} {(weeks == 1 ? "week" : "weeks")} ago";
        }
        if (days < 365)
        {
            var months = days / 30;
            return $"{months} {(months == 1 ? "month" : "months")} ago";
        }

        var years = days / 365;
        return $"{years} {(years == 1 ? "year" : "years")} ago";
    }

    private async void OnExportClicked(object? sender, EventArgs e)
    {
        if (exporting) return;
        if (!Screenshot.Default.IsCaptureSupported)
        {
            ShowError("Screenshot capture isn't supported on this device.");
            return;
        }

        exporting = true;
        var exportButtonWasVisible = ExportButton.IsVisible;
        var exportButtonWasEnabled = ExportButton.IsEnabled;
        var tabBarWasVisible = Shell.GetTabBarIsVisible(this);
        try
        {
            ExportButton.IsEnabled = false;
            ExportButton.IsVisible = false;
            Shell.SetTabBarIsVisible(this, false);
            await Task.Delay(100);

            var screenshot = await Screenshot.Default.CaptureAsync();
            var filePath = System.IO.Path.Combine(
                FileSystem.CacheDirectory,
                $"quick-stats-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}.png");
            using (var source = await screenshot.OpenReadAsync())
            using (var destination = File.Create(filePath))
                await source.CopyToAsync(destination);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Quick Stats screenshot",
                File = new ShareFile(filePath)
            });
        }
        catch (Exception ex)
        {
            ShowError($"Screenshot export failed: {ex.Message}");
        }
        finally
        {
            ExportButton.IsVisible = exportButtonWasVisible;
            ExportButton.IsEnabled = exportButtonWasEnabled;
            Shell.SetTabBarIsVisible(this, tabBarWasVisible);
            exporting = false;
        }
    }

    private sealed class ReportTrendDrawable : IDrawable
    {
        private const float left = 46;
        private const float top = 8;
        private const float right = 12;
        private const float bottom = 28;
        private const float vertexHitRadius = 18;
        private IReadOnlyList<DailyReportCount> dailyCounts = Array.Empty<DailyReportCount>();

        public void SetData(IReadOnlyList<DailyReportCount> counts) => dailyCounts = counts;

        public DailyReportCount? FindVertexAt(PointF touch, RectF bounds)
        {
            if (dailyCounts.Count == 0 || bounds.Width <= 0 || bounds.Height <= 0)
                return null;

            var axisMax = GetAxisMax();
            var closestDistance = vertexHitRadius * vertexHitRadius;
            DailyReportCount? closest = null;

            for (var index = 0; index < dailyCounts.Count; index++)
            {
                if (!IsVertex(index))
                    continue;

                var point = GetPoint(index, bounds, axisMax);
                var deltaX = point.X - touch.X;
                var deltaY = point.Y - touch.Y;
                var distance = deltaX * deltaX + deltaY * deltaY;
                if (distance <= closestDistance)
                {
                    closestDistance = distance;
                    closest = dailyCounts[index];
                }
            }

            return closest;
        }

        public void Draw(ICanvas canvas, RectF bounds)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            var graphWidth = bounds.Width - left - right;
            var graphHeight = bounds.Height - top - bottom;
            var axisMax = GetAxisMax();
            var tickStep = Math.Max(1, (int)Math.Ceiling(axisMax / 10.0));

            canvas.StrokeColor = Color.FromArgb("#E7EDF4");
            canvas.StrokeSize = 1;
            for (var count = axisMax; count >= 0; count -= tickStep)
            {
                var y = top + graphHeight * (1 - (float)count / axisMax);
                canvas.DrawLine(left, y, left + graphWidth, y);

                canvas.FontColor = Color.FromArgb("#526579");
                canvas.FontSize = 11;
                canvas.DrawString(
                    count.ToString(),
                    4,
                    y - 7,
                    left - 8,
                    14,
                    HorizontalAlignment.Right,
                    VerticalAlignment.Center);
            }

            canvas.StrokeColor = Color.FromArgb("#9AAABD");
            canvas.StrokeSize = 1;
            canvas.DrawLine(left, top, left, top + graphHeight);
            canvas.DrawLine(left, top + graphHeight, left + graphWidth, top + graphHeight);

            var xTickCount = Math.Min(6, dailyCounts.Count);
            for (var tick = 0; tick < xTickCount; tick++)
            {
                var position = xTickCount == 1 ? 0f : tick / (float)(xTickCount - 1);
                var index = (int)Math.Round(position * (dailyCounts.Count - 1));
                var x = GetPoint(index, bounds, axisMax).X;
                canvas.StrokeColor = Color.FromArgb("#9AAABD");
                canvas.StrokeSize = 1;
                canvas.DrawLine(x, top + graphHeight, x, top + graphHeight + 5);
                canvas.FontColor = Color.FromArgb("#526579");
                canvas.FontSize = 9;
                const float labelWidth = 56;
                var labelX = x - labelWidth / 2;
                var alignment = HorizontalAlignment.Center;
                if (xTickCount > 1 && tick == 0)
                {
                    labelX = x;
                    alignment = HorizontalAlignment.Left;
                }
                else if (xTickCount > 1 && tick == xTickCount - 1)
                {
                    labelX = x - labelWidth;
                    alignment = HorizontalAlignment.Right;
                }
                canvas.DrawString(
                    dailyCounts[index].Date.ToString("MMM. dd"),
                    labelX,
                    top + graphHeight + 7,
                    labelWidth,
                    16,
                    alignment,
                    VerticalAlignment.Top);
            }

            var points = dailyCounts
                .Select((_, index) => GetPoint(index, bounds, axisMax))
                .ToArray();

            canvas.StrokeColor = Color.FromArgb("#087E8B");
            canvas.StrokeSize = 3;
            for (var index = 1; index < points.Length; index++)
                canvas.DrawLine(points[index - 1], points[index]);

            canvas.FillColor = Color.FromArgb("#087E8B");
            for (var index = 0; index < points.Length; index++)
            {
                if (IsVertex(index))
                    canvas.FillCircle(points[index], 4);
            }
        }

        private int GetAxisMax()
        {
            var observedMaxCount = dailyCounts.Count == 0
                ? 1
                : Math.Max(1, dailyCounts.Max(day => Math.Max(0, day.Count)));
            var tickStep = Math.Max(1, (int)Math.Ceiling(observedMaxCount / 10.0));
            return (int)Math.Ceiling(observedMaxCount / (double)tickStep) * tickStep;
        }

        private PointF GetPoint(int index, RectF bounds, int axisMax)
        {
            var graphWidth = bounds.Width - left - right;
            var graphHeight = bounds.Height - top - bottom;
            var x = left + graphWidth * index / Math.Max(1, dailyCounts.Count - 1);
            var y = top + graphHeight * (1 - (float)Math.Max(0, dailyCounts[index].Count) / axisMax);
            return new PointF(x, y);
        }

        private bool IsVertex(int index) =>
            index == 0 ||
            index == dailyCounts.Count - 1 ||
            (index > 0 && dailyCounts[index - 1].Count != dailyCounts[index].Count) ||
            (index + 1 < dailyCounts.Count && dailyCounts[index].Count != dailyCounts[index + 1].Count);
    }
}
