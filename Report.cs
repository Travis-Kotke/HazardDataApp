namespace UWO_Safety_App;

// A single safety issue reported by a user.
public class Report
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Severity { get; set; } = ""; // Low, Medium, High
    public string Location { get; set; } = "";
    public string ReportedBy { get; set; } = "";
    public DateTime DateReported { get; set; }

    public bool IsResolved { get; set; }
    public DateTime? DateResolved { get; set; }
    public string ResolutionNotes { get; set; } = "";

    public string StatusText => IsResolved ? "Resolved" : "Unresolved";
}
