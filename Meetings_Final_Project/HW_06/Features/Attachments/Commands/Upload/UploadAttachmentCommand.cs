using HW_06.DTOs.Files;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace HW_06.Features.Attachments.Commands.Upload;

/// <summary>
/// Команда для завантаження
/// публічного файлу до зустрічі.
/// </summary>
/// <param name="MeetingId">
/// Ідентифікатор зустрічі.
/// </param>
/// <param name="File">
/// Файл, який необхідно завантажити.
/// </param>
public record UploadAttachmentCommand(
    int MeetingId,
    IFormFile File)
    : IRequest<AttachmentPublicDTO?>;