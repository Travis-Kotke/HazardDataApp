// Author: Eric Puksich
// Page: Unresolved Issues Page
// Description: Lists all open (unresolved) safety reports, newest first.
//              Tapping a report opens it in the Issue Detail page.
namespace UWO_Safety_App;

public partial class UnresolvedIssuesPage : ContentPage
{
    public UnresolvedIssuesPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Reload every time so issues resolved in the advanced view drop off this list.
        IssuesList.ItemsSource = ReportData.Unresolved;
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
