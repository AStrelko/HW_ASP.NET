using HW_06.DTOs.ParticipantDTO;
using HW_06.Helpers.Pagination;
using HW_06.Helpers.QueryParameters;
using MediatR;

namespace HW_06.Features.Participants.Queries.GetAll;

/// <summary>
/// Запит для отримання списку учасників
/// із підтримкою пошуку, сортування та пагінації.
/// </summary>
/// <param name="Parameters">
/// Параметри пошуку, сортування та пагінації.
/// </param>
public record GetAllParticipantsQuery(
    ParticipantQueryParameters Parameters)
    : IRequest<PagedResult<ParticipantReadDTO>>;