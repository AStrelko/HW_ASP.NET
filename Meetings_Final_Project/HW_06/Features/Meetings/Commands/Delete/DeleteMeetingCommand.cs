using MediatR;

namespace HW_06.Features.Meetings.Commands.Delete;

/// <summary>
/// Команда для видалення зустрічі
/// за її ідентифікатором.
/// </summary>
/// <param name="Id">
/// Ідентифікатор зустрічі.
/// </param>
public record DeleteMeetingCommand(
    int Id)
    : IRequest<bool>;