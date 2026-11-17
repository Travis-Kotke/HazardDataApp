using HazardDataApp.BusinessLogic.Models;

namespace HazardDataApp.UI.Views;

// Display-only formatting stays outside the business and database layers.
public sealed class ReportRow(Report report, int index)
{
    public Report Report { get; } = report;
    public Color Background => Color.FromArgb(index % 2 == 0 ? "#F5F5F8" : "#FFFFFF");
    public string PublicLabel => Report.Published ? "Yes" : "No";
    public string StatusLabel => Report.IsResolved ? "Resolved" : "Unresolved";
    public string ResolutionAction => Report.IsResolved ? "Reopen" : "Resolve";
    public string FlagLabel => Report.IsFlagged ? "Unflag" : "Flag";
    public Color FlagColor => Color.FromArgb(Report.IsFlagged ? "#A84420" : "#565663");
    public Color SeverityColor => Color.FromArgb(Report.Severity switch
    {
        "High" => "#AC2525",
        "Intermediate" => "#825500",
        _ => "#326246"
    });
}
