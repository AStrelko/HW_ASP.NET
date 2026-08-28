using MediatR;

namespace HW_06.Features.Meetings.Commands.DeleteMany;

/// <summary>
/// Команда для видалення декількох зустрічей
/// за списком ідентифікаторів.
/// </summary>
/// <param name="Ids">
/// Список ідентифікаторів зустрічей.
/// </param>
public record DeleteManyMeetingsCommand(
    List<int> Ids)
    : IRequest<int>;