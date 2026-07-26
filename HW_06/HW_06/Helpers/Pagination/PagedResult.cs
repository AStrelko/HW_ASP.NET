namespace HW_06.Helpers.Pagination;

/// <summary>
/// Результат запиту з пагінацією.
/// </summary>
public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize)
{
    /// <summary>
    /// Загальна кількість сторінок.
    /// </summary>
    public int TotalPages =>
        (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Чи існує попередня сторінка.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Чи існує наступна сторінка.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}