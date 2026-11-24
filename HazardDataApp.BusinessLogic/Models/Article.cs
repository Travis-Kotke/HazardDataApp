namespace HazardDataApp.BusinessLogic.Models;

// Proposed article model only; no article persistence until the table is approved.
public sealed class Article
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public string Author { get; set; } = "";
    public DateTimeOffset DatePublished { get; set; }
}
