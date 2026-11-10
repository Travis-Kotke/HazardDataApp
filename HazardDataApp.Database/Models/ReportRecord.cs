using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace HazardDataApp.Database.Models;

// Table mapping stays in Database; UI never consumes this Supabase model.
[Table("reports")]
public sealed class ReportRecord : BaseModel
{
    [PrimaryKey("id", false)] public int Id { get; set; }
    [Column("title")] public string Title { get; set; } = "";
    [Column("description")] public string Description { get; set; } = "";
    [Column("severity")] public string Severity { get; set; } = "";
    [Column("location")] public string Location { get; set; } = "";
    [Column("is_inside")] public bool IsInside { get; set; }
    [Column("floor")] public string? Floor { get; set; }
    [Column("reported_by")] public string ReportedBy { get; set; } = "";
    [Column("date_reported")] public DateTime DateReported { get; set; }
    [Column("is_resolved")] public bool IsResolved { get; set; }
    [Column("date_resolved")] public DateTime? DateResolved { get; set; }
    [Column("resolution_notes")] public string ResolutionNotes { get; set; } = "";
}
