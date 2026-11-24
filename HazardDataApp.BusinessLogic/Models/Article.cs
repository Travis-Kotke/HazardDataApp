
namespace HazardDataApp.BusinessLogic.Models;

public sealed class Article
{
    public long Id { get; set; }

    public string Title { get; set; } = "";

    public string Body { get; set; } = "";

    public bool IsPublished { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string RowColor { get; set; } = "#FFFFFF";
}
