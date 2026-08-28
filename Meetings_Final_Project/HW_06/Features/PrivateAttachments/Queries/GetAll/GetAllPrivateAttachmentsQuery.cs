using HW_06.DTOs.Files;
using MediatR;

namespace HW_06.Features.PrivateAttachments.Queries.GetAll;

/// <summary>
/// Запит для отримання списку
/// всіх приватних файлів.
/// </summary>
public record GetAllPrivateAttachmentsQuery
    : IRequest<IReadOnlyCollection<AttachmentPrivateDTO>>;