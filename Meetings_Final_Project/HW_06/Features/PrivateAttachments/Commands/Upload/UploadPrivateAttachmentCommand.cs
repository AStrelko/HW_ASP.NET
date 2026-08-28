using HW_06.DTOs.Files;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace HW_06.Features.PrivateAttachments.Commands.Upload;

/// <summary>
/// Команда для надсилання приватного файлу
/// від одного учасника іншому.
/// </summary>
/// <param name="SenderParticipantId">
/// Ідентифікатор учасника-відправника.
/// </param>
/// <param name="RecipientParticipantId">
/// Ідентифікатор учасника-отримувача.
/// </param>
/// <param name="File">
/// Файл для надсилання.
/// </param>
public record UploadPrivateAttachmentCommand(
    int SenderParticipantId,
    int RecipientParticipantId,
    IFormFile File)
    : IRequest<AttachmentPrivateDTO?>;