using MediatR;

namespace HW_06.Features.Participants.Commands.DeleteMany;

/// <summary>
/// Команда для видалення декількох учасників
/// за списком ідентифікаторів.
/// </summary>
/// <param name="Ids">
/// Список ідентифікаторів учасників.
/// </param>
public record DeleteManyParticipantsCommand(
    List<int> Ids)
    : IRequest<int>;