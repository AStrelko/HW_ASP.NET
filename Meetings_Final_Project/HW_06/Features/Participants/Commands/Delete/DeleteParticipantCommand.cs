using MediatR;

namespace HW_06.Features.Participants.Commands.Delete;

/// <summary>
/// Команда для видалення учасника
/// за його ідентифікатором.
/// </summary>
/// <param name="Id">
/// Ідентифікатор учасника.
/// </param>
public record DeleteParticipantCommand(
    int Id)
    : IRequest<bool>;