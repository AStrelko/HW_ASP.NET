using HW_06.DTOs.Files;
using HW_06.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HW_06.Controllers;

/// <summary>
/// Керує публічними файлами-вкладеннями зустрічей.
/// </summary>
[ApiController]
[Route("api/meetings/{meetingId:int}/attachments")]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentService _attachmentService;

    /// <summary>
    /// Ініціалізує новий екземпляр контролера
    /// публічних файлів-вкладень зустрічей.
    /// </summary>
    /// <param name="attachmentService">
    /// Сервіс для роботи з публічними файлами-вкладеннями.
    /// </param>
    public AttachmentsController(
        IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
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
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(
        typeof(AttachmentPublicDTO),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AttachmentPublicDTO>> Upload(
        int meetingId,
        IFormFile file)
    {
        var attachment =
            await _attachmentService.UploadAsync(
                meetingId,
                file);

        if (attachment is null)
        {
            return NotFound(new
            {
                message =
                    $"Зустріч з ідентифікатором {meetingId} не знайдена."
            });
        }

        return Created(
            attachment.DownloadUrl,
            attachment);
    }

    /// <summary>
    /// Повертає список публічних документів,
    /// прикріплених до зустрічі.
    /// </summary>
    /// <param name="meetingId">
    /// Ідентифікатор зустрічі.
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
    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<AttachmentPublicDTO>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<
        IReadOnlyCollection<AttachmentPublicDTO>>> GetAll(
        int meetingId)
    {
        var attachments =
            await _attachmentService.GetAllAsync(meetingId);

        if (attachments is null)
        {
            return NotFound(new
            {
                message =
                    $"Зустріч з ідентифікатором {meetingId} не знайдена."
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
    /// <response code="204">
    /// Документ успішно видалено.
    /// </response>
    /// <response code="404">
    /// Документ не знайдено або він не належить указаній зустрічі.
    /// </response>
    [HttpDelete("{attachmentId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int meetingId,
        int attachmentId)
    {
        var deleted = await _attachmentService.DeleteAsync(
            meetingId,
            attachmentId);

        if (!deleted)
        {
            return NotFound(new
            {
                message =
                    $"Документ з ідентифікатором {attachmentId} " +
                    $"не знайдено для зустрічі {meetingId}."
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
    /// <response code="200">
    /// Документ успішно отримано.
    /// </response>
    /// <response code="404">
    /// Документ не знайдено або файл відсутній у сховищі.
    /// </response>
    [HttpGet("{attachmentId:int}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(
        int meetingId,
        int attachmentId)
    {
        var document = await _attachmentService.DownloadAsync(
            meetingId,
            attachmentId);

        if (document is null)
        {
            return NotFound(new
            {
                message =
                    $"Документ з ідентифікатором {attachmentId} " +
                    $"не знайдено для зустрічі {meetingId}."
            });
        }

        return File(
            document.Content,
            document.ContentType,
            document.OriginalFileName);
    }
}