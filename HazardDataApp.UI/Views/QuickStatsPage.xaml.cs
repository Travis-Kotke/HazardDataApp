using HazardDataApp.BusinessLogic.Services;

namespace HazardDataApp.UI.Views;

public partial class QuickStatsPage : ContentPage
{
    private readonly IReportService reportService;
    private bool loading;

    public QuickStatsPage(IReportService reportService)
    {
        InitializeComponent();
        this.reportService = reportService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async void OnRefreshClicked(object? sender, EventArgs e) => await LoadAsync();

    private async Task LoadAsync()
    {
        if (loading) return;
        loading = true;
        RefreshButton.IsEnabled = false;
        StatusLabel.Text = "Loading reports…";
        TotalLabel.Text = "Total: —";
        OpenLabel.Text = "Open: —";
        ResolvedLabel.Text = "Resolved: —";
        try
        {
            var stats = await reportService.GetStatisticsAsync();
            TotalLabel.Text = $"Total: {stats.Total}";
            OpenLabel.Text = $"Open: {stats.Open}";
            ResolvedLabel.Text = $"Resolved: {stats.Resolved}";
            StatusLabel.Text = "Counts reflect reports visible to the current Supabase role.";
        }
        catch (InvalidOperationException ex)
        {
            StatusLabel.Text = ex.Message;
        }
        catch (Exception)
        {
            StatusLabel.Text = "Could not load reports. Check your connection, Supabase settings, and reports read policy, then refresh.";
        }
        finally
        {
            loading = false;
            RefreshButton.IsEnabled = true;
        }
    }
}
