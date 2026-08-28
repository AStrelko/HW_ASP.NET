using HW_06.DTOs.Files;
using HW_06.Features.Participants.Queries.GetIdByUserId;
using HW_06.Features.PrivateAttachments.Commands.Delete;
using HW_06.Features.PrivateAttachments.Commands.DeleteByAdmin;
using HW_06.Features.PrivateAttachments.Commands.Upload;
using HW_06.Features.PrivateAttachments.Queries.Download;
using HW_06.Features.PrivateAttachments.Queries.GetAll;
using HW_06.Features.PrivateAttachments.Queries.GetById;
using HW_06.Features.PrivateAttachments.Queries.GetReceived;
using HW_06.Features.PrivateAttachments.Queries.GetSent;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using HW_06.Common.Constants;

namespace HW_06.Controllers;

/// <summary>
/// Керує приватними файлами учасників.
/// </summary>
[ApiController]
[Authorize]
[Route("api/participants/{participantId:int}/private-files")]
public class PrivateAttachmentsController : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>
    /// Ініціалізує новий екземпляр контролера
    /// приватних файлів учасників.
    /// </summary>
    /// <param name="sender">
    /// Сервіс MediatR для надсилання команд і запитів.
    /// </param>
    public PrivateAttachmentsController(ISender sender)
    {
        ArgumentNullException.ThrowIfNull(sender);
        _sender = sender;
    }

    /// <summary>
    /// Повертає ідентифікатор учасника,
    /// пов'язаного з поточним авторизованим користувачем.
    /// </summary>
    private async Task<int?> GetCurrentParticipantIdAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return await _sender.Send(new GetParticipantIdByUserIdQuery(userId), cancellationToken);
    }

    /// <summary>
    /// Надсилає приватний файл
    /// від одного учасника іншому.
    /// </summary>
    [HttpPost("send")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 11 * 1024 * 1024)]
    [ProducesResponseType(typeof(AttachmentPrivateDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    public async Task<ActionResult<AttachmentPrivateDTO>> Upload(int participantId, [FromForm] int recipientParticipantId,
            IFormFile file, CancellationToken cancellationToken)
    {
        var currentParticipantId = await GetCurrentParticipantIdAsync(cancellationToken);

        if (currentParticipantId != participantId)
        {
            return Forbid();
        }

        var uploadedFile = await _sender.Send(new UploadPrivateAttachmentCommand(
                    participantId, recipientParticipantId, file), cancellationToken);

        if (uploadedFile is null)
        {
            return NotFound(new
            {
                message = "Відправника або отримувача не знайдено."
            });
        }

        return Created(uploadedFile.DownloadUrl, uploadedFile);
    }

    /// <summary>
    /// Повертає приватні файли,
    /// отримані вказаним учасником.
    /// </summary>
    [HttpGet("received")]
    [ProducesResponseType(typeof(IReadOnlyCollection<AttachmentPrivateDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<AttachmentPrivateDTO>>> GetReceived(
            int participantId, CancellationToken cancellationToken)
    {
        var currentParticipantId = await GetCurrentParticipantIdAsync(cancellationToken);

        if (currentParticipantId != participantId)
        {
            return Forbid();
        }

        var files = await _sender.Send(new GetReceivedPrivateAttachmentsQuery(
                    participantId), cancellationToken);

        return Ok(files);
    }

    /// <summary>
    /// Повертає приватні файли,
    /// надіслані вказаним учасником.
    /// </summary>
    [HttpGet("sent")]
    [ProducesResponseType(typeof(IReadOnlyCollection<AttachmentPrivateDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<AttachmentPrivateDTO>>> GetSent(int participantId,
            CancellationToken cancellationToken)
    {
        var currentParticipantId = await GetCurrentParticipantIdAsync(cancellationToken);

        if (currentParticipantId != participantId)
        {
            return Forbid();
        }

        var files = await _sender.Send(new GetSentPrivateAttachmentsQuery(
                    participantId), cancellationToken);

        return Ok(files);
    }

    /// <summary>
    /// Повертає інформацію
    /// про конкретний приватний файл.
    /// </summary>
    [HttpGet("{fileId:int}")]
    [ProducesResponseType(typeof(AttachmentPrivateDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AttachmentPrivateDTO>> GetById(int participantId, int fileId,
            CancellationToken cancellationToken)
    {
        var currentParticipantId = await GetCurrentParticipantIdAsync(cancellationToken);

        if (currentParticipantId != participantId)
        {
            return Forbid();
        }

        var privateFile = await _sender.Send(new GetPrivateAttachmentByIdQuery(fileId, participantId),
                cancellationToken);

        if (privateFile is null)
        {
            return NotFound(new
            {
                message = $"Приватний файл з ідентифікатором {fileId} не знайдено або доступ заборонено."
            });
        }

        return Ok(privateFile);
    }

    /// <summary>
    /// Завантажує приватний файл.
    /// </summary>
    [HttpGet("{fileId:int}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(int participantId, int fileId, CancellationToken cancellationToken)
    {
        var currentParticipantId = await GetCurrentParticipantIdAsync(cancellationToken);
        if (currentParticipantId != participantId)
        {
            return Forbid();
        }

        var document = await _sender.Send(new DownloadPrivateAttachmentQuery(
                    fileId, participantId), cancellationToken);

        if (document is null)
        {
            return NotFound(new
            {
                message = $"Приватний файл з ідентифікатором {fileId} не знайдено або доступ заборонено."
            });
        }

        return File(document.Content, document.ContentType, document.OriginalFileName);
    }

    /// <summary>
    /// Видаляє приватний файл.
    /// </summary>
    [HttpDelete("{fileId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int participantId, int fileId, CancellationToken cancellationToken)
    {
        bool deleted;

        if (User.IsInRole("Admin"))
        {
            deleted = await _sender.Send(new DeletePrivateAttachmentByAdminCommand(fileId), cancellationToken);
        }
        else
        {
            var currentParticipantId = await GetCurrentParticipantIdAsync(cancellationToken);

            if (currentParticipantId != participantId)
            {
                return Forbid();
            }

            deleted = await _sender.Send(new DeletePrivateAttachmentCommand(fileId, participantId), cancellationToken);
        }

        if (!deleted)
        {
            return NotFound(new
            {
                message = $"Приватний файл з ідентифікатором {fileId} не знайдено або видалення заборонено."
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Повертає список усіх приватних файлів.
    /// Доступно лише адміністратору.
    /// </summary>
    [HttpGet("/api/private-files")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    [ProducesResponseType(typeof(IReadOnlyCollection<AttachmentPrivateDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<
        ActionResult<IReadOnlyCollection<AttachmentPrivateDTO>>> GetAll(CancellationToken cancellationToken)
    {
        var files = await _sender.Send(new GetAllPrivateAttachmentsQuery(),
                cancellationToken);

        return Ok(files);
    }
}