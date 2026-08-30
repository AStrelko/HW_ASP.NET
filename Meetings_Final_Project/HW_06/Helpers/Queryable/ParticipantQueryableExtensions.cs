using HW_06.Helpers.QueryParameters;
using HW_06.Models;

namespace HW_06.Helpers.Queryable;

/// <summary>
/// Методи розширення для пошуку
/// та сортування учасників.
/// </summary>
public static class ParticipantQueryableExtensions
{
    /// <summary>
    /// Застосовує пошук учасників
    /// за ім'ям, прізвищем або email.
    /// </summary>
    /// <param name="query">
    /// Вихідна послідовність учасників.
    /// </param>
    /// <param name="search">
    /// Пошуковий рядок.
    /// </param>
    /// <returns>
    /// Запит із застосованим пошуком.
    /// </returns>
    public static IQueryable<Participant> ApplySearch(
        this IQueryable<Participant> query,
        string? search)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var searchValue =
            search.Trim();

        return query.Where(participant =>
            participant.FirstName.Contains(searchValue) ||
            participant.LastName.Contains(searchValue) ||
            (participant.ApplicationUser != null &&
             participant.ApplicationUser.Email != null &&
             participant.ApplicationUser.Email.Contains(
                 searchValue)));
    }

    /// <summary>
    /// Застосовує сортування учасників
    /// відповідно до заданих параметрів.
    /// </summary>
    /// <param name="query">
    /// Вихідна послідовність учасників.
    /// </param>
    /// <param name="parameters">
    /// Параметри сортування.
    /// </param>
    /// <returns>
    /// Відсортований запит учасників.
    /// </returns>
    public static IQueryable<Participant> ApplySorting(
        this IQueryable<Participant> query,
        ParticipantQueryParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(parameters);

        var sortBy =
            parameters.SortBy?
                .Trim()
                .ToLowerInvariant();

        return sortBy switch
        {
            "id" or "participantid" =>
                parameters.Descending
                    ? query.OrderByDescending(participant =>
                        participant.ParticipantId)
                    : query.OrderBy(participant =>
                        participant.ParticipantId),

            "firstname" =>
                parameters.Descending
                    ? query
                        .OrderByDescending(participant =>
                            participant.FirstName)
                        .ThenBy(participant =>
                            participant.ParticipantId)
                    : query
                        .OrderBy(participant =>
                            participant.FirstName)
                        .ThenBy(participant =>
                            participant.ParticipantId),

            "lastname" =>
                parameters.Descending
                    ? query
                        .OrderByDescending(participant =>
                            participant.LastName)
                        .ThenBy(participant =>
                            participant.ParticipantId)
                    : query
                        .OrderBy(participant =>
                            participant.LastName)
                        .ThenBy(participant =>
                            participant.ParticipantId),

            "email" =>
                parameters.Descending
                    ? query
                        .OrderByDescending(participant =>
                            participant.ApplicationUser!.Email)
                        .ThenBy(participant =>
                            participant.ParticipantId)
                    : query
                        .OrderBy(participant =>
                            participant.ApplicationUser!.Email)
                        .ThenBy(participant =>
                            participant.ParticipantId),

            "position" =>
                parameters.Descending
                    ? query
                        .OrderByDescending(participant =>
                            participant.Position)
                        .ThenBy(participant =>
                            participant.ParticipantId)
                    : query
                        .OrderBy(participant =>
                            participant.Position)
                        .ThenBy(participant =>
                            participant.ParticipantId),

            _ =>
                parameters.Descending
                    ? query
                        .OrderByDescending(participant =>
                            participant.ParticipantId)
                    : query
                        .OrderBy(participant =>
                            participant.ParticipantId)
        };
    }
}