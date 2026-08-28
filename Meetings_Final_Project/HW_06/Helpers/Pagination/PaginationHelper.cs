using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Helpers.Pagination;

/// <summary>
/// Допоміжні методи для пагінації запитів.
/// </summary>
public static class PaginationHelper
{
    /// <summary>
    /// Перетворює запит на результат із пагінацією
    /// та виконує перетворення сутностей у DTO.
    /// </summary>
    /// <typeparam name="TEntity">
    /// Тип сутності бази даних.
    /// </typeparam>
    /// <typeparam name="TDto">
    /// Тип DTO, який повертається клієнту.
    /// </typeparam>
    /// <param name="query">Запит до бази даних.</param>
    /// <param name="pageNumber">Номер сторінки.</param>
    /// <param name="pageSize">
    /// Кількість елементів на сторінці.
    /// </param>
    /// <param name="mapper">Екземпляр AutoMapper.</param>
    /// <returns>Результат із пагінацією.</returns>
    public static async Task<PagedResult<TDto>>
        ToPagedResultAsync<TEntity, TDto>(
            this IQueryable<TEntity> query,
            int pageNumber,
            int pageSize,
            IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(mapper);

        pageNumber = pageNumber < 1
            ? 1
            : pageNumber;

        pageSize = pageSize < 1
            ? 10
            : pageSize;

        var totalCount = await query.CountAsync();

        var entities = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = mapper.Map<List<TDto>>(entities);

        return new PagedResult<TDto>(
            items,
            totalCount,
            pageNumber,
            pageSize);
    }
}