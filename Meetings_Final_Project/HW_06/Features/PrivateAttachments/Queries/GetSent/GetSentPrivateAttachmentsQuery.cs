using HW_06.DTOs.Files;
using MediatR;

namespace HW_06.Features.PrivateAttachments.Queries.GetSent;

/// <summary>
/// Запит для отримання приватних файлів,
/// надісланих зазначеним учасником.
/// </summary>
/// <param name="ParticipantId">
/// Ідентифікатор учасника-відправника.
/// </param>
public record GetSentPrivateAttachmentsQuery(
    int ParticipantId)
    : IRequest<IReadOnlyCollection<AttachmentPrivateDTO>>;