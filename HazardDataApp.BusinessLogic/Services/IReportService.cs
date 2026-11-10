using HazardDataApp.BusinessLogic.Models;

namespace HazardDataApp.BusinessLogic.Services;

public interface IReportService
{
    Task<ReportStatistics> GetStatisticsAsync();
}
