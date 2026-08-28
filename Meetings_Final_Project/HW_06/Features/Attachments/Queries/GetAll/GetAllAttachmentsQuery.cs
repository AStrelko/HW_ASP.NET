using HW_06.DTOs.Files;
using MediatR;

namespace HW_06.Features.Attachments.Queries.GetAll;

/// <summary>
/// Запит для отримання всіх
/// публічних файлів зазначеної зустрічі.
/// </summary>
/// <param name="MeetingId">
/// Ідентифікатор зустрічі.
/// </param>
public record GetAllAttachmentsQuery(
    int MeetingId)
    : IRequest<IReadOnlyCollection<AttachmentPublicDTO>?>;