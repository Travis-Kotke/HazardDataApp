using HazardDataApp.BusinessLogic.Models;

namespace HazardDataApp.BusinessLogic.Services;

public interface IReportService
{
    Task<IReadOnlyList<Report>> GetReportsAsync();
    Task<Report> SetFlaggedAsync(int id, bool isFlagged);
    Task<ReportStatistics> GetStatisticsAsync();
    Task<QuickStatsData> GetQuickStatsAsync(int? numberOfDays = null);
    Task<IReadOnlyList<Article>> GetArticlesAsync();
    Task<Article> AddArticleAsync(string title, string body);
    Task<Article> SetArticlePublishedAsync(long id, bool isPublished);
    Task DeleteArticleAsync(long id);
}
