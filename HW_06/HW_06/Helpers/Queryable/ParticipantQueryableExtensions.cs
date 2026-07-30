using HW_06.Helpers.QueryParameters;
using HW_06.Models;

namespace HW_06.Helpers.Queryable;

/// <summary>
/// Методи розширення для пошуку та сортування учасників.
/// </summary>
public static class ParticipantQueryableExtensions
{
    /// <summary>
    /// Застосовує пошук учасників за прізвищем.
    /// </summary>
    /// <param name="query">
    /// Вихідна послідовність учасників.
    /// </param>
    /// <param name="searchLastName">
    /// Частина або повне прізвище для пошуку.
    /// Якщо значення не вказано, пошук не застосовується.
    /// </param>
    /// <returns>
    /// Відфільтрована послідовність учасників або вихідна колекція,
    /// якщо параметр пошуку не заданий.
    /// </returns>
    public static IQueryable<Participant> ApplySearch(
        this IQueryable<Participant> query,
        string? searchLastName)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrWhiteSpace(searchLastName))
        {
            return query;
        }

        var search = searchLastName.Trim();

        return query.Where(participant =>
            participant.LastName.Contains(search));
    }

    /// <summary>
    /// Застосовує сортування учасників відповідно
    /// до заданих параметрів.
    /// </summary>
    /// <param name="query">
    /// Вихідна послідовність учасників.
    /// </param>
    /// <param name="parameters">
    /// Параметри сортування.
    /// </param>
    /// <returns>
    /// Послідовність учасників із застосованим сортуванням.
    /// Якщо параметри сортування не задані,
    /// використовується сортування за ідентифікатором.
    /// </returns>

    public static IQueryable<Participant> ApplySorting(
        this IQueryable<Participant> query,
        ParticipantQueryParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(parameters);

        var sortBy = parameters.SortBy?
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
                        .ThenByDescending(participant =>
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
                        .ThenByDescending(participant =>
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
                            participant.Email)
                        .ThenByDescending(participant =>
                            participant.ParticipantId)
                    : query
                        .OrderBy(participant =>
                            participant.Email)
                        .ThenBy(participant =>
                            participant.ParticipantId),

            "role" =>
                parameters.Descending
                    ? query
                        .OrderByDescending(participant =>
                            participant.Role)
                        .ThenByDescending(participant =>
                            participant.ParticipantId)
                    : query
                        .OrderBy(participant =>
                            participant.Role)
                        .ThenBy(participant =>
                            participant.ParticipantId),

            _ => parameters.Descending
                ? query.OrderByDescending(participant =>
                    participant.ParticipantId)
                : query.OrderBy(participant =>
                    participant.ParticipantId)
        };
    }
}