using HazardDataApp.Database.Models;

namespace HazardDataApp.Database.Repositories;

internal sealed class ReportRepository(SupabaseConnection connection) : IReportRepository
{
    public async Task<IReadOnlyList<ReportRecord>> GetAllAsync()
    {
        var client = await connection.GetClientAsync();
        var reports = new List<ReportRecord>();
        const int pageSize = 500;
        // PostgREST limits responses; page through all rows for accurate totals.
        for (var offset = 0; ;)
        {
            var response = await client.From<ReportRecord>()
                .Order("id", Supabase.Postgrest.Constants.Ordering.Ascending)
                .Range(offset, offset + pageSize - 1).Get();
            if (response.Models.Count == 0) break;
            reports.AddRange(response.Models);
            offset += response.Models.Count;
        }
        return reports;
    }
}
