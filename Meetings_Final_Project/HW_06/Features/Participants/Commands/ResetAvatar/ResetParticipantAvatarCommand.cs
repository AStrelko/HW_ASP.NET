using MediatR;

namespace HW_06.Features.Participants.Commands.ResetAvatar;

/// <summary>
/// Команда для видалення власного аватара
/// учасника та повернення до стандартного аватара.
/// </summary>
/// <param name="ParticipantId">
/// Ідентифікатор учасника.
/// </param>
public record ResetParticipantAvatarCommand(
    int ParticipantId)
    : IRequest<bool>;