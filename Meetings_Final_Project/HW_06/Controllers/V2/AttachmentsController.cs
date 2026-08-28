using HW_06.Common.Constants;
using HW_06.DTOs.Files;
using HW_06.Features.Attachments.Commands.Delete;
using HW_06.Features.Attachments.Commands.Upload;
using HW_06.Features.Attachments.Queries.Download;
using HW_06.Features.Attachments.Queries.GetAll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HW_06.Controllers;

/// <summary>
/// Керує публічними файлами-вкладеннями зустрічей.
/// </summary>
[ApiController]
[Route("api/meetings/{meetingId:int}/attachments")]
public class AttachmentsController : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>
    /// Ініціалізує новий екземпляр контролера
    /// публічних файлів-вкладень зустрічей.
    /// </summary>
    /// <param name="sender">
    /// Сервіс MediatR для надсилання команд і запитів.
    /// </param>
    public AttachmentsController(ISender sender)
    {
        ArgumentNullException.ThrowIfNull(sender);
        _sender = sender;
    }

    /// <summary>
    /// Завантажує публічний файл
    /// і прикріплює його до зустрічі.
    /// </summary>
    /// <param name="meetingId">
    /// Ідентифікатор зустрічі.
    /// </param>
    /// <param name="file">
    /// Файл, який потрібно завантажити.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
    /// <returns>
    /// Дані створеного файлу-вкладення.
    /// </returns>
    /// <response code="201">
    /// Файл успішно завантажено.
    /// </response>
    /// <response code="400">
    /// Файл відсутній, порожній або не пройшов перевірку.
    /// </response>
    /// <response code="404">
    /// Зустріч не знайдено.
    /// </response>
    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(AttachmentPublicDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AttachmentPublicDTO>>
        Upload(int meetingId, IFormFile file, CancellationToken cancellationToken)
    {
        var attachment = await _sender.Send(new UploadAttachmentCommand(meetingId, file), cancellationToken);

        if (attachment is null)
        {
            return NotFound(new
            {
                message = $"Зустріч з ідентифікатором {meetingId} не знайдена."
            });
        }

        return Created(attachment.DownloadUrl, attachment);
    }

    /// <summary>
    /// Повертає список публічних документів,
    /// прикріплених до зустрічі.
    /// </summary>
    /// <param name="meetingId">
    /// Ідентифікатор зустрічі.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
    /// <returns>
    /// Список документів зустрічі.
    /// </returns>
    /// <response code="200">
    /// Список документів успішно отримано.
    /// </response>
    /// <response code="404">
    /// Зустріч не знайдено.
    /// </response>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<AttachmentPublicDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<AttachmentPublicDTO>>>
        GetAll(int meetingId, CancellationToken cancellationToken)
    {
        var attachments = await _sender.Send(new GetAllAttachmentsQuery(meetingId),
                cancellationToken);

        if (attachments is null)
        {
            return NotFound(new
            {
                message = $"Зустріч з ідентифікатором {meetingId} не знайдена."
            });
        }
        return Ok(attachments);
    }

    /// <summary>
    /// Видаляє публічний документ,
    /// прикріплений до зустрічі.
    /// </summary>
    /// <param name="meetingId">
    /// Ідентифікатор зустрічі.
    /// </param>
    /// <param name="attachmentId">
    /// Ідентифікатор документа.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
    /// <response code="204">
    /// Документ успішно видалено.
    /// </response>
    /// <response code="404">
    /// Документ не знайдено або він
    /// не належить указаній зустрічі.
    /// </response>
    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpDelete("{attachmentId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int meetingId, int attachmentId, CancellationToken cancellationToken)
    {
        var deleted = await _sender.Send(new DeleteAttachmentCommand(meetingId, attachmentId), cancellationToken);

        if (!deleted)
        {
            return NotFound(new
            {
                message = $"Документ з ідентифікатором {attachmentId} не знайдено для зустрічі {meetingId}."
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Відкриває або завантажує документ,
    /// прикріплений до зустрічі.
    /// </summary>
    /// <param name="meetingId">
    /// Ідентифікатор зустрічі.
    /// </param>
    /// <param name="attachmentId">
    /// Ідентифікатор документа.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
    /// <response code="200">
    /// Документ успішно отримано.
    /// </response>
    /// <response code="404">
    /// Документ не знайдено або файл
    /// відсутній у сховищі.
    /// </response>
    [Authorize]
    [HttpGet("{attachmentId:int}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(int meetingId, int attachmentId, CancellationToken cancellationToken)
    {
        var document = await _sender.Send(
                new DownloadAttachmentQuery(meetingId, attachmentId), cancellationToken);

        if (document is null)
        {
            return NotFound(new
            {
                message = $"Документ з ідентифікатором {attachmentId} не знайдено для зустрічі {meetingId}."
            });
        }

        return File(document.Content, document.ContentType, document.OriginalFileName);
    }
}