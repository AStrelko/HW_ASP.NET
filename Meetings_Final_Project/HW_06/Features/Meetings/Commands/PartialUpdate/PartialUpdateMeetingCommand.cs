using HW_06.DTOs.MeetingDTO;
using MediatR;

namespace HW_06.Features.Meetings.Commands.PartialUpdate;

/// <summary>
/// Команда для часткового оновлення зустрічі.
/// </summary>
/// <param name="Id">
/// Ідентифікатор зустрічі.
/// </param>
/// <param name="Dto">
/// Поля зустрічі, які необхідно оновити.
/// </param>
public record PartialUpdateMeetingCommand(
    int Id,
    MeetingPartialUpdateDTO Dto)
    : IRequest<bool>;