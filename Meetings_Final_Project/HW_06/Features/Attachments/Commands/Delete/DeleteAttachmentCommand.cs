using MediatR;

namespace HW_06.Features.Attachments.Commands.Delete;

/// <summary>
/// Команда для видалення публічного файлу,
/// прикріпленого до зустрічі.
/// </summary>
/// <param name="MeetingId">
/// Ідентифікатор зустрічі.
/// </param>
/// <param name="AttachmentId">
/// Ідентифікатор публічного файлу.
/// </param>
public record DeleteAttachmentCommand(
    int MeetingId,
    int AttachmentId)
    : IRequest<bool>;