using HazardDataApp.Database.Models;

namespace HazardDataApp.Database.Repositories;

internal sealed class ReportRepository(SupabaseConnection connection) : IReportRepository
{
    public async Task<ReportRecord> SetFlaggedAsync(int id, bool isFlagged)
    {
        var client = await connection.GetClientAsync();
        // Patch only this field on this report, preserving unrelated edits.
        var response = await client.From<ReportRecord>()
            .Where(report => report.Id == id)
            .Set(report => report.IsFlagged, isFlagged)
            .Update();
        return response.Models.SingleOrDefault()
            ?? throw new InvalidOperationException("The report could not be updated. It may have been removed, or your account may not have permission.");
    }

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

    public async Task<IReadOnlyList<ArticleRecord>> GetArticlesAsync()
    {
        var client = await connection.GetClientAsync();

        var response = await client.From<ArticleRecord>()
            .Order("created_at",
                Supabase.Postgrest.Constants.Ordering.Descending)
            .Get();

        return response.Models;
    }

    public async Task<ArticleRecord> AddArticleAsync(
        string title, string body)
    {
        var client = await connection.GetClientAsync();

        var response = await client.From<ArticleRecord>().Insert(
            new ArticleRecord
            {
                Title = title,
                Body = body,
                IsPublished = false,
                CreatedAt = DateTimeOffset.UtcNow
            });

        return response.Models.Single();
    }

    public async Task<ArticleRecord> SetArticlePublishedAsync(
        long id, bool isPublished)
    {
        var client = await connection.GetClientAsync();

        var response = await client.From<ArticleRecord>()
            .Where(article => article.Id == id)
            .Set(article => article.IsPublished, isPublished)
            .Update();

        return response.Models.Single();
    }

    public async Task DeleteArticleAsync(long id)
    {
        var client = await connection.GetClientAsync();

        await client.From<ArticleRecord>()
            .Where(article => article.Id == id)
            .Delete();
    }
}
