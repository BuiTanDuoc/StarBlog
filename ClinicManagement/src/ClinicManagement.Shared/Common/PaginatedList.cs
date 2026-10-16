using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Shared.Common;

/// <summary>
/// Standard paged result shape returned by every list endpoint. Build with
/// CreateAsync from a filtered/sorted IQueryable inside a query handler
/// (Phase 3) — it issues exactly two queries (COUNT + page) regardless of
/// table size.
/// </summary>
public class PaginatedList<T>
{
    public List<T> Items { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages { get; }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = count;
        TotalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(count / (double)pageSize);
    }

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize is < 1 or > 200 ? 20 : pageSize; // 200 = hard ceiling to prevent abuse

        var count = await source.CountAsync();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}
