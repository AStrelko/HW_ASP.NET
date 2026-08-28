using HW_06.Services.Results;
using MediatR;

namespace HW_06.Features.Attachments.Queries.Download;

/// <summary>
/// Запит для завантаження
/// публічного файлу зустрічі.
/// </summary>
/// <param name="MeetingId">
/// Ідентифікатор зустрічі.
/// </param>
/// <param name="AttachmentId">
/// Ідентифікатор вкладення.
/// </param>
public record DownloadAttachmentQuery(
    int MeetingId,
    int AttachmentId)
    : IRequest<AttachmentDownloadResult?>;