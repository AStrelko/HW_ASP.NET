using HW_06.DTOs.MeetingDTO;
using MediatR;

namespace HW_06.Features.Meetings.Queries.GetByParticipant;

/// <summary>
/// Запит для отримання всіх зустрічей,
/// у яких бере участь зазначений учасник.
/// </summary>
/// <param name="ParticipantId">
/// Ідентифікатор учасника.
/// </param>
public record GetMeetingsByParticipantQuery(
    int ParticipantId)
    : IRequest<List<MeetingReadDTO>>;