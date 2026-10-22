// Author: Eric Puksich
// Page: Resolved Issues Page
// Description: Lists all resolved safety reports, most recently resolved first.
//              Tapping a report opens it in the Issue Detail page.
namespace UWO_Safety_App;

public partial class ResolvedIssuesPage : ContentPage
{
    public ResolvedIssuesPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Reload every time so newly resolved issues show up.
        IssuesList.ItemsSource = ReportData.Resolved;
    }

    private async void IssuesList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not Report report)
            return;

        // Clear the selection so the same issue can be tapped again after coming back.
        IssuesList.SelectedItem = null;
        await Navigation.PushAsync(new IssueDetailPage(report));
    }
}
