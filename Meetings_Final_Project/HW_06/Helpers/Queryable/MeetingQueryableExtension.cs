using HW_06.Helpers.QueryParameters;
using HW_06.Models;

namespace HW_06.Helpers.Queryable;

/// <summary>
/// Методи розширення для фільтрації запитів зустрічей.
/// </summary>
public static class MeetingQueryableExtension
{
    /// <summary>
    /// Застосовує фільтри за датою та номером кімнати.
    /// </summary>
    /// <param name="query">Запит зустрічей.</param>
    /// <param name="filter">Параметри фільтрації.</param>
    /// <returns>Відфільтрований запит.</returns>
    public static IQueryable<Meeting> ApplyFilter(
        this IQueryable<Meeting> query,
        MeetingFilter filter)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(filter);

        if (filter.StartTime.HasValue)
        {
            query = query.Where(meeting =>
                meeting.DateTime >= filter.StartTime.Value);
        }

        if (filter.EndTime.HasValue)
        {
            query = query.Where(meeting =>
                meeting.DateTime <= filter.EndTime.Value);
        }

        if (filter.RoomNumber.HasValue)
        {
            query = query.Where(meeting =>
                meeting.Room != null &&
                meeting.Room.NumberRoom ==
                filter.RoomNumber.Value);
        }

        return query;
    }
}