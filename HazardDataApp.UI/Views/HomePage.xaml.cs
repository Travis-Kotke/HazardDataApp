using HazardDataApp.BusinessLogic.Models;
using HazardDataApp.BusinessLogic.Services;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace HazardDataApp.UI.Views;

public partial class HomePage : ContentPage
{
    private readonly IReportService reportService;
    private IReadOnlyList<Report> reports = [];
    private List<ReportRow> visibleRows = [];
    private ReportRow? selectionAnchor;
    private bool ready;
    private bool busy;
    private bool loaded;
    private bool wideLayout;

    public HomePage(IReportService reportService)
    {
        InitializeComponent();
        this.reportService = reportService;
        SeverityPicker.ItemsSource = new[] { "All severities", "Low", "Intermediate", "High" };
        ResolutionPicker.ItemsSource = new[] { "All reports", "Unresolved", "Resolved" };
        SeverityPicker.SelectedIndex = 0;
        ResolutionPicker.SelectedIndex = 0;
        FromDatePicker.Date = DateTime.Today.AddDays(-30);
        ToDatePicker.Date = DateTime.Today;
        ready = true;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async void OnRefreshClicked(object? sender, EventArgs e) => await LoadAsync();

    private async void OnExportClicked(object? sender, EventArgs e)
    {
        var selectedReports = visibleRows
            .Where(row => row.IsSelected)
            .Select(row => row.Report)
            .ToList();
        if (selectedReports.Count == 0)
        {
            StatusLabel.Text = "Select one or more table rows before exporting.";
            return;
        }

        var format = await DisplayActionSheetAsync("Choose an export format", "Cancel", null, "CSV (.csv)", "Excel (.xlsx)");
        if (format is not ("CSV (.csv)" or "Excel (.xlsx)")) return;

        try
        {
            var filePath = format == "CSV (.csv)"
                ? await ReportExporter.ExportCsvAsync(selectedReports)
                : await ReportExporter.ExportExcelAsync(selectedReports);
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = $"Hazard reports ({selectedReports.Count})",
                File = new ShareFile(filePath)
            });
            StatusLabel.Text = $"Exported {selectedReports.Count} selected report(s).";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Export failed: {ex.Message}";
        }
    }

    private async Task LoadAsync()
    {
        if (busy) return;
        SetBusy(true);
        StatusLabel.Text = "Loading reports...";
        try
        {
            reports = await reportService.GetReportsAsync();
            loaded = true;
            ApplyFilters();
        }
        catch (Exception ex)
        {
            StatusLabel.Text = loaded
                ? "Refresh failed. Showing previously loaded reports. Try Refresh again."
                : ex is InvalidOperationException ? ex.Message
                : "Couldn't load reports. Check your connection and try Refresh again.";
            if (!loaded) EmptyLabel.Text = "Reports unavailable. Check your connection and report access, then tap Refresh.";
        }
        finally { SetBusy(false); }
    }

    private void OnFilterChanged(object? sender, EventArgs e)
    {
        if (ready && loaded) ApplyFilters();
    }

    private void OnDateRangeChanged(object? sender, ToggledEventArgs e)
    {
        if (!ready) return;
        DatesGrid.IsEnabled = e.Value;
        DatesGrid.IsVisible = e.Value;
        if (loaded) ApplyFilters();
    }

    private void OnClearFiltersClicked(object? sender, EventArgs e)
    {
        ready = false;
        TitleEntry.Text = "";
        SeverityPicker.SelectedIndex = 0;
        ResolutionPicker.SelectedIndex = 0;
        PublishedSwitch.IsToggled = false;
        UseDateRange.IsToggled = false;
        DatesGrid.IsEnabled = false;
        DatesGrid.IsVisible = false;
        ready = true;
        if (loaded) ApplyFilters();
    }

    private void ApplyFilters()
    {
        try
        {
            var filter = new ReportFilter
            {
                Title = TitleEntry.Text ?? "",
                Severity = SeverityPicker.SelectedIndex > 0 ? (string)SeverityPicker.SelectedItem : null,
                IsResolved = ResolutionPicker.SelectedIndex switch { 1 => false, 2 => true, _ => null },
                PublishedOnly = PublishedSwitch.IsToggled,
                FromDate = UseDateRange.IsToggled ? FromDatePicker.Date : null,
                ToDate = UseDateRange.IsToggled ? ToDatePicker.Date : null
            };
            var filtered = filter.Apply(reports);
            visibleRows = filtered.Select((report, index) => new ReportRow(report, index)).ToList();
            selectionAnchor = null;
            ReportsList.ItemsSource = visibleRows;
            ResultsLabel.Text = $"Hazard reports ({filtered.Count} of {reports.Count})";
            EmptyLabel.Text = reports.Count == 0
                ? "No reports are available to your account yet."
                : "No matching reports. Try changing or clearing your filters.";
            StatusLabel.Text = "Click rows to select or clear them. Shift-click another row to select a range.";
        }
        catch (ArgumentException ex)
        {
            visibleRows = [];
            selectionAnchor = null;
            ReportsList.ItemsSource = Array.Empty<ReportRow>();
            ResultsLabel.Text = "Invalid date range";
            EmptyLabel.Text = ex.Message;
            StatusLabel.Text = ex.Message;
        }
    }

    private void OnReportSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not ReportRow row) return;

        var shiftPressed = IsShiftPressed();
        var rowIndex = visibleRows.IndexOf(row);
        var anchorIndex = selectionAnchor is null ? -1 : visibleRows.IndexOf(selectionAnchor);
        if (rowIndex < 0) return;

        if (shiftPressed && anchorIndex >= 0 && rowIndex >= 0)
        {
            var first = Math.Min(anchorIndex, rowIndex);
            var last = Math.Max(anchorIndex, rowIndex);
            for (var index = 0; index < visibleRows.Count; index++)
                visibleRows[index].IsSelected = index >= first && index <= last;
        }
        else
        {
            row.IsSelected = !row.IsSelected;
            selectionAnchor = row;
        }

        var selectedCount = visibleRows.Count(visibleRow => visibleRow.IsSelected);
        StatusLabel.Text = $"{selectedCount} report(s) selected. Shift-click another row to select a range.";
        ReportsList.SelectedItem = null;
    }

    private static bool IsShiftPressed()
    {
#if WINDOWS
        return (GetAsyncKeyState(0x10) & 0x8000) != 0;
#else
        return false;
#endif
    }

#if WINDOWS
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int virtualKey);
#endif

    private async void OnFlagClicked(object? sender, EventArgs e)
    {
        if (busy || sender is not ImageButton { BindingContext: ReportRow row }) return;
        SetBusy(true);
        StatusLabel.Text = "Saving flag...";
        try
        {
            var updated = await reportService.SetFlaggedAsync(row.Report.Id, !row.Report.IsFlagged);
            reports = reports.Select(report => report.Id == updated.Id ? updated : report).ToList();
            ApplyFilters();
            StatusLabel.Text = updated.IsFlagged ? "Report flagged for review." : "Report flag removed.";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = ex is InvalidOperationException ? ex.Message
                : "Couldn't save the flag. Try again, or check your report update permissions.";
        }
        finally { SetBusy(false); }
    }

    private void SetBusy(bool value)
    {
        busy = value;
        RefreshButton.IsEnabled = !value;
        ReportsList.IsEnabled = !value;
        FiltersPanel.IsEnabled = !value;
    }

    private void OnBodySizeChanged(object? sender, EventArgs e)
    {
        if (!ready) return;
        var wide = BodyGrid.Width >= 960;
        if (wide != wideLayout || BodyGrid.ColumnDefinitions.Count == 0)
        {
            wideLayout = wide;
            BodyGrid.ColumnDefinitions.Clear();
            BodyGrid.RowDefinitions.Clear();
            BodyGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            if (wide) BodyGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(380)));
            BodyGrid.RowDefinitions.Add(new RowDefinition(wide ? GridLength.Star : GridLength.Auto));
            if (!wide) BodyGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            Grid.SetColumn(FiltersPanel, wide ? 1 : 0);
            Grid.SetRow(ReportsPanel, wide ? 0 : 1);
        }
        FiltersScroll.HeightRequest = wide ? -1 : Math.Min(260, BodyGrid.Height * 0.45);
        TableGrid.WidthRequest = Math.Max(680, wide ? BodyGrid.Width - 402 : BodyGrid.Width - 2);
    }
}
