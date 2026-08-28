using HW_06.Helpers.QueryParameters;
using MediatR;
using HW_06.DTOs.MeetingDTO;
using HW_06.Helpers.Pagination;

namespace HW_06.Features.Meetings.Queries.GetAll;

/// <summary>
/// Запит для отримання списку зустрічей
/// із підтримкою пошуку, фільтрації,
/// сортування та пагінації.
/// </summary>
/// <param name="Filter">
/// Параметри фільтрації зустрічей.
/// </param>
/// <param name="Parameters">
/// Параметри пошуку, сортування та пагінації.
/// </param>
public record GetAllMeetingsQuery(
    MeetingFilter Filter,
    MeetingQueryParameters Parameters)
    : IRequest<PagedResult<MeetingReadDTO>>;