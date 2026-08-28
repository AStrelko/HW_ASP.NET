using HW_06.DTOs.MeetingDTO;
using MediatR;

namespace HW_06.Features.Meetings.Commands.Update;

/// <summary>
/// Команда для повного оновлення зустрічі.
/// </summary>
/// <param name="Id">
/// Ідентифікатор зустрічі.
/// </param>
/// <param name="Dto">
/// Нові дані зустрічі.
/// </param>
public record UpdateMeetingCommand(
    int Id,
    MeetingUpdateDTO Dto)
    : IRequest<bool>;