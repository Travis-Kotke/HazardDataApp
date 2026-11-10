using HazardDataApp.Database.Models;

namespace HazardDataApp.Database.Repositories;

public interface IReportRepository
{
    Task<IReadOnlyList<ReportRecord>> GetAllAsync();
}
