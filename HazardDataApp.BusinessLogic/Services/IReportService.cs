using HazardDataApp.BusinessLogic.Models;

namespace HazardDataApp.BusinessLogic.Services;

public interface IReportService
{
    Task<IReadOnlyList<Report>> GetReportsAsync();
    Task<Report> SetFlaggedAsync(int id, bool isFlagged);
    Task<ReportStatistics> GetStatisticsAsync();
    Task<QuickStatsData> GetQuickStatsAsync(int? numberOfDays = null);
}
