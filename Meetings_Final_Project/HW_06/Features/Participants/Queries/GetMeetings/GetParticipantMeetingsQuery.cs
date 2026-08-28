using HW_06.DTOs.MeetingDTO;
using MediatR;

namespace HW_06.Features.Participants.Queries.GetMeetings;

/// <summary>
/// Запит для отримання списку зустрічей,
/// у яких бере участь зазначений учасник.
/// </summary>
/// <param name="ParticipantId">
/// Ідентифікатор учасника.
/// </param>
public record GetParticipantMeetingsQuery(
    int ParticipantId)
    : IRequest<List<MeetingReadDTO>>;