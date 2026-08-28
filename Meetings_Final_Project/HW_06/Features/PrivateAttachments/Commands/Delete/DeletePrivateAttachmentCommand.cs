using MediatR;

namespace HW_06.Features.PrivateAttachments.Commands.Delete;

/// <summary>
/// Команда для видалення приватного файлу
/// його відправником.
/// </summary>
/// <param name="FileId">
/// Ідентифікатор приватного файлу.
/// </param>
/// <param name="ParticipantId">
/// Ідентифікатор учасника-відправника.
/// </param>
public record DeletePrivateAttachmentCommand(
    int FileId,
    int ParticipantId)
    : IRequest<bool>;