using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Helpers.Pagination;

/// <summary>
/// Допоміжні методи для пагінації запитів.
/// </summary>
public static class PaginationHelper
{
    /// <summary>
    /// Перетворює запит на результат із пагінацією
    /// та проєктує сутності безпосередньо в DTO.
    /// </summary>
    /// <typeparam name="TEntity">
    /// Тип сутності бази даних.
    /// </typeparam>
    /// <typeparam name="TDto">
    /// Тип DTO, який повертається клієнту.
    /// </typeparam>
    /// <param name="query">
    /// Запит до бази даних.
    /// </param>
    /// <param name="pageNumber">
    /// Номер сторінки.
    /// </param>
    /// <param name="pageSize">
    /// Кількість елементів на сторінці.
    /// </param>
    /// <param name="mapper">
    /// Екземпляр AutoMapper.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування асинхронної операції.
    /// </param>
    /// <returns>
    /// Результат із пагінацією.
    /// </returns>
    public static async Task<PagedResult<TDto>>
        ToPagedResultAsync<TEntity, TDto>(
            this IQueryable<TEntity> query,
            int pageNumber,
            int pageSize,
            IMapper mapper,
            CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<TDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

        return new PagedResult<TDto>(items, totalCount, pageNumber, pageSize);
    }
}