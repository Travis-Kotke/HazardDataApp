using System.Collections.ObjectModel;
using HazardDataApp.BusinessLogic.Models;
using HazardDataApp.BusinessLogic.Services;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace HazardDataApp.UI.Views;

public sealed record RecentReportRow(string Title, Color BackgroundColor);

public partial class QuickStatsPage : ContentPage
{
    private readonly IReportService reportService;
    private readonly ObservableCollection<RecentReportRow> recentReports = new();
    private bool loading;

    public QuickStatsPage(IReportService reportService)
    {
        InitializeComponent();
        this.reportService = reportService;
        RecentReportsList.ItemsSource = recentReports;
        TrendChart.Drawable = new ReportTrendDrawable();
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
                index % 2 == 0 ? Color.FromArgb("#F3F7FB") : Color.FromArgb("#FAFBFD")));
        }

        if (recentReports.Count == 0)
            recentReports.Add(new RecentReportRow("No accidents reported.", Color.FromArgb("#F3F7FB")));

        ((ReportTrendDrawable)TrendChart.Drawable!).SetData(data.DailyCounts);
        TrendChart.Invalidate();
    }

    private void ShowError(string message)
    {
        StatusLabel.Text = message;
        StatusLabel.IsVisible = true;
    }

    private async void OnExportClicked(object sender, EventArgs e)
    {
        await DisplayActionSheet("Choose an export format", "Cancel", null, "CSV", "Excel");
    }

    private sealed class ReportTrendDrawable : IDrawable
    {
        private IReadOnlyList<DailyReportCount> dailyCounts = Array.Empty<DailyReportCount>();

        public void SetData(IReadOnlyList<DailyReportCount> counts) => dailyCounts = counts;

        public void Draw(ICanvas canvas, RectF bounds)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            const float left = 8;
            const float top = 8;
            const float right = 8;
            const float bottom = 8;
            var graphWidth = bounds.Width - left - right;
            var graphHeight = bounds.Height - top - bottom;

            canvas.StrokeColor = Color.FromArgb("#E7EDF4");
            canvas.StrokeSize = 1;
            for (var gridLine = 0; gridLine <= 3; gridLine++)
            {
                var y = top + graphHeight * gridLine / 3;
                canvas.DrawLine(left, y, left + graphWidth, y);
            }

            canvas.StrokeColor = Color.FromArgb("#9AAABD");
            canvas.StrokeSize = 1;
            canvas.DrawLine(left, top, left, top + graphHeight);
            canvas.DrawLine(left, top + graphHeight, left + graphWidth, top + graphHeight);

            if (dailyCounts.Count == 0)
                return;

            var maxCount = Math.Max(1, dailyCounts.Max(day => Math.Max(0, day.Count)));
            var points = dailyCounts.Select((day, index) =>
            {
                var x = left + graphWidth * index / Math.Max(1, dailyCounts.Count - 1);
                var y = top + graphHeight * (1 - (float)Math.Max(0, day.Count) / maxCount);
                return new PointF(x, y);
            }).ToArray();

            canvas.StrokeColor = Color.FromArgb("#087E8B");
            canvas.StrokeSize = 3;
            for (var index = 1; index < points.Length; index++)
                canvas.DrawLine(points[index - 1], points[index]);

            canvas.FillColor = Color.FromArgb("#087E8B");
            foreach (var point in points)
                canvas.FillCircle(point, 4);
        }
    }
}
