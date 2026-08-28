using HW_06.DTOs.Files;
using MediatR;

namespace HW_06.Features.PrivateAttachments.Queries.GetById;

/// <summary>
/// Запит для отримання інформації
/// про конкретний приватний файл.
/// </summary>
/// <param name="FileId">
/// Ідентифікатор приватного файлу.
/// </param>
/// <param name="ParticipantId">
/// Ідентифікатор учасника,
/// який запитує файл.
/// </param>
public record GetPrivateAttachmentByIdQuery(
    int FileId,
    int ParticipantId)
    : IRequest<AttachmentPrivateDTO?>;