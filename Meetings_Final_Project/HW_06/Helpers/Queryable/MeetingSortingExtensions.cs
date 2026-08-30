using HW_06.Helpers.QueryParameters;
using HW_06.Models;

namespace HW_06.Helpers.Queryable;

/// <summary>
/// Розширення для пошуку та сортування зустрічей.
/// </summary>
public static class MeetingSortingExtensions
{
    /// <summary>
    /// Застосовує пошук за назвою
    /// та описом зустрічі.
    /// </summary>
    /// <param name="query">
    /// Запит зустрічей.
    /// </param>
    /// <param name="search">
    /// Пошуковий рядок.
    /// </param>
    /// <returns>
    /// Запит із застосованим пошуком.
    /// </returns>
    public static IQueryable<Meeting> ApplySearch(
        this IQueryable<Meeting> query,
        string? search)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var searchValue =
            search.Trim();

        return query.Where(meeting =>
            meeting.Title.Contains(searchValue) ||
            meeting.Description.Contains(searchValue));
    }

    /// <summary>
    /// Застосовує сортування зустрічей.
    /// </summary>
    /// <param name="query">
    /// Запит зустрічей.
    /// </param>
    /// <param name="parameters">
    /// Параметри сортування.
    /// </param>
    /// <returns>
    /// Відсортований запит.
    /// </returns>
    public static IQueryable<Meeting> ApplySorting(
        this IQueryable<Meeting> query,
        MeetingQueryParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(parameters);

        var sortBy =
            parameters.SortBy?
                .Trim()
                .ToLowerInvariant();

        return sortBy switch
        {
            "id" or "meetingid" =>
                parameters.Descending
                    ? query.OrderByDescending(meeting =>
                        meeting.MeetingId)
                    : query.OrderBy(meeting =>
                        meeting.MeetingId),

            "title" =>
                parameters.Descending
                    ? query
                        .OrderByDescending(meeting =>
                            meeting.Title)
                        .ThenBy(meeting =>
                            meeting.MeetingId)
                    : query
                        .OrderBy(meeting =>
                            meeting.Title)
                        .ThenBy(meeting =>
                            meeting.MeetingId),

            "date" or "datetime" =>
                parameters.Descending
                    ? query
                        .OrderByDescending(meeting =>
                            meeting.DateTime)
                        .ThenBy(meeting =>
                            meeting.MeetingId)
                    : query
                        .OrderBy(meeting =>
                            meeting.DateTime)
                        .ThenBy(meeting =>
                            meeting.MeetingId),

            "room" or "roomnumber" =>
                parameters.Descending
                    ? query
                        .OrderByDescending(meeting =>
                            meeting.Room != null
                                ? meeting.Room.NumberRoom
                                : 0)
                        .ThenBy(meeting =>
                            meeting.MeetingId)
                    : query
                        .OrderBy(meeting =>
                            meeting.Room != null
                                ? meeting.Room.NumberRoom
                                : 0)
                        .ThenBy(meeting =>
                            meeting.MeetingId),

            "participants" or "participantscount" =>
                parameters.Descending
                    ? query
                        .OrderByDescending(meeting =>
                            meeting.MeetingParticipants.Count)
                        .ThenBy(meeting =>
                            meeting.MeetingId)
                    : query
                        .OrderBy(meeting =>
                            meeting.MeetingParticipants.Count)
                        .ThenBy(meeting =>
                            meeting.MeetingId),

            _ =>
                parameters.Descending
                    ? query
                        .OrderByDescending(meeting =>
                            meeting.DateTime)
                        .ThenBy(meeting =>
                            meeting.MeetingId)
                    : query
                        .OrderBy(meeting =>
                            meeting.DateTime)
                        .ThenBy(meeting =>
                            meeting.MeetingId)
        };
    }
}