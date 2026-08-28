using HW_06.Services.Results;
using MediatR;

namespace HW_06.Features.PrivateAttachments.Queries.Download;

/// <summary>
/// Запит для завантаження
/// приватного файлу учасником.
/// </summary>
/// <param name="FileId">
/// Ідентифікатор приватного файлу.
/// </param>
/// <param name="ParticipantId">
/// Ідентифікатор учасника,
/// який завантажує файл.
/// </param>
public record DownloadPrivateAttachmentQuery(
    int FileId,
    int ParticipantId)
    : IRequest<AttachmentDownloadResult?>;