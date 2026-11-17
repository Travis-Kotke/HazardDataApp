namespace HazardDataApp.BusinessLogic.Models;

// UI-facing report data has no Supabase dependency or table attributes.
public sealed record Report(
    int Id,
    string Title,
    string Description,
    string Severity,
    string Location,
    bool IsInside,
    string? Floor,
    string ReportedBy,
    DateTime DateReported,
    bool IsResolved,
    DateTime? DateResolved,
    string ResolutionNotes,
    bool Published,
    bool IsFlagged);
