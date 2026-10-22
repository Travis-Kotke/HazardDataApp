// Author: Eric Puksich
// Page: Issue Detail Page
// Description: Shows the full details of a single report.
//              Unresolved reports can be marked as resolved with notes.
namespace UWO_Safety_App;

public partial class IssueDetailPage : ContentPage
{
    private readonly Report _report;

    public IssueDetailPage(Report report)
    {
        InitializeComponent();
        _report = report;
        BindingContext = report;

        ResolutionSection.IsVisible = report.IsResolved;
        ResolveSection.IsVisible = !report.IsResolved;
    }

    private async void MarkResolved_Clicked(object? sender, EventArgs e)
    {
        // Updates the placeholder data only; nothing is saved until there is a database.
        _report.IsResolved = true;
        _report.DateResolved = DateTime.Now;
        _report.ResolutionNotes = ResolutionNotesEditor.Text?.Trim() ?? "";

        await DisplayAlertAsync("Issue Resolved", $"\"{_report.Title}\" was marked as resolved.", "OK");
        await Navigation.PopAsync();
    }
}
