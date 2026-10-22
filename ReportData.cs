namespace UWO_Safety_App;

// Placeholder in-memory reports for the prototype. Replace with the database once it exists.
// Changes (like resolving a report) only last until the app is closed.
public static class ReportData
{
    public static List<Report> All { get; } = new()
    {
        new Report
        {
            Id = 1,
            Title = "Icy sidewalk outside library",
            Description = "The sidewalk by the main entrance has a sheet of ice that hasn't been salted. Saw two people slip this morning.",
            Severity = "High",
            Location = "Polk Library",
            ReportedBy = "Student",
            DateReported = new DateTime(2026, 10, 19, 8, 15, 0),
        },
        new Report
        {
            Id = 2,
            Title = "Blocked fire exit",
            Description = "Stack of chairs is blocking the east stairwell fire exit on the second floor.",
            Severity = "High",
            Location = "Reeve Union",
            ReportedBy = "Staff",
            DateReported = new DateTime(2026, 10, 18, 14, 40, 0),
        },
        new Report
        {
            Id = 3,
            Title = "Broken light in parking lot",
            Description = "Two of the overhead lights in the north end of the lot are out. Very dark walking to cars at night.",
            Severity = "Medium",
            Location = "Lot 13",
            ReportedBy = "Student",
            DateReported = new DateTime(2026, 10, 17, 21, 5, 0),
        },
        new Report
        {
            Id = 4,
            Title = "Space heater left on",
            Description = "A space heater was left running overnight under a desk next to a pile of papers.",
            Severity = "High",
            Location = "Sage Hall, Room 2210",
            ReportedBy = "Faculty",
            DateReported = new DateTime(2026, 10, 12, 7, 50, 0),
            IsResolved = true,
            DateResolved = new DateTime(2026, 10, 12, 10, 30, 0),
            ResolutionNotes = "Heater removed and department reminded of the space heater policy.",
        },
        new Report
        {
            Id = 5,
            Title = "Loose handrail on stairs",
            Description = "Handrail on the south stairs wobbles and feels like it could pull out of the wall.",
            Severity = "Medium",
            Location = "Halsey Science Center",
            ReportedBy = "Student",
            DateReported = new DateTime(2026, 10, 8, 11, 20, 0),
            IsResolved = true,
            DateResolved = new DateTime(2026, 10, 10, 15, 0, 0),
            ResolutionNotes = "Facilities re-anchored the handrail.",
        },
    };

    public static List<Report> Unresolved =>
        All.Where(r => !r.IsResolved).OrderByDescending(r => r.DateReported).ToList();

    public static List<Report> Resolved =>
        All.Where(r => r.IsResolved).OrderByDescending(r => r.DateResolved).ToList();
}
