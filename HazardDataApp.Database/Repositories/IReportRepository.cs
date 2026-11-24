using HazardDataApp.Database.Models;

namespace HazardDataApp.Database.Repositories;

public interface IReportRepository
{
    Task<IReadOnlyList<ReportRecord>> GetAllAsync();
    Task<ReportRecord> SetFlaggedAsync(int id, bool isFlagged);
    Task<IReadOnlyList<ArticleRecord>> GetArticlesAsync();
    Task<ArticleRecord> AddArticleAsync(string title, string body);
    Task<ArticleRecord> SetArticlePublishedAsync(long id, bool isPublished);
    Task DeleteArticleAsync(long id);
}

