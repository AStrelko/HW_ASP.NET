using HW_06.DTOs.Files;
using MediatR;

namespace HW_06.Features.PrivateAttachments.Queries.GetReceived;

/// <summary>
/// Запит для отримання приватних файлів,
/// отриманих зазначеним учасником.
/// </summary>
/// <param name="ParticipantId">
/// Ідентифікатор учасника-отримувача.
/// </param>
public record GetReceivedPrivateAttachmentsQuery(
    int ParticipantId)
    : IRequest<IReadOnlyCollection<AttachmentPrivateDTO>>;