using HW_06.DTOs.MeetingDTO;
using MediatR;

namespace HW_06.Features.Meetings.Commands.Create;

/// <summary>
/// Команда для створення нової зустрічі.
/// </summary>
/// <param name="Dto">
/// Дані нової зустрічі.
/// </param>
/// <param name="OrganizerId">
/// Ідентифікатор користувача Identity,
/// який створює зустріч.
/// </param>
public record CreateMeetingCommand(
    MeetingCreateDTO Dto,
    string OrganizerId)
    : IRequest<MeetingReadDTO>;