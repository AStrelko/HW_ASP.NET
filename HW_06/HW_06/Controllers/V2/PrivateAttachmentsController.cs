using HW_06.DTOs.Files;
using HW_06.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HW_06.Controllers;

/// <summary>
/// Керує приватними файлами учасників.
/// </summary>
[ApiController]
[Route("api/participants/{participantId:int}/private-files")]
public class PrivateAttachmentsController : ControllerBase
{
    private readonly IPrivateAttachmentService _privateAttachmentService;

    /// <summary>
    /// Ініціалізує новий екземпляр контролера
    /// приватних файлів учасників.
    /// </summary>
    /// <param name="privateAttachmentService">
    /// Сервіс роботи з приватними файлами.
    /// </param>
    public PrivateAttachmentsController(
        IPrivateAttachmentService privateAttachmentService)
    {
        _privateAttachmentService = privateAttachmentService;
    }

    /// <summary>
    /// Надсилає приватний файл від одного учасника іншому.
    /// </summary>
    /// <param name="participantId">
    /// Ідентифікатор учасника-відправника.
    /// </param>
    /// <param name="recipientParticipantId">
    /// Ідентифікатор учасника-отримувача.
    /// </param>
    /// <param name="file">
    /// Файл для надсилання.
    /// </param>
    /// <response code="201">
    /// Файл успішно надіслано.
    /// </response>
    /// <response code="400">
    /// Дані або файл не пройшли перевірку.
    /// Дозволені формати: PDF, DOCX і TXT.
    /// Максимальний розмір файлу — 10 МБ.
    /// </response>
    /// <response code="404">
    /// Відправника або отримувача не знайдено.
    /// </response>
    /// <response code="413">
    /// Розмір HTTP-запиту перевищує допустиме значення.
    /// </response>
    [HttpPost("send")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    [RequestFormLimits(
        MultipartBodyLengthLimit = 11 * 1024 * 1024)]
    [ProducesResponseType(
        typeof(AttachmentPrivateDTO),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        StatusCodes.Status413PayloadTooLarge)]
    public async Task<ActionResult<AttachmentPrivateDTO>> Upload(
        int participantId,
        [FromForm] int recipientParticipantId,
        IFormFile file)
    {
        var uploadedFile =
            await _privateAttachmentService.UploadAsync(
                participantId,
                recipientParticipantId,
                file);

        if (uploadedFile is null)
        {
            return NotFound(new
            {
                message =
                    "Відправника або отримувача не знайдено."
            });
        }

        return Created(
            uploadedFile.DownloadUrl,
            uploadedFile);
    }
    /// <summary>
    /// Повертає приватні файли,
    /// отримані вказаним учасником.
    /// </summary>
    /// <param name="participantId">
    /// Ідентифікатор учасника-отримувача.
    /// </param>
    /// <returns>
    /// Колекція приватних файлів, отриманих учасником.
    /// </returns>
    /// <response code="200">
    /// Список отриманих приватних файлів успішно сформовано.
    /// </response>
    [HttpGet("received")]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<AttachmentPrivateDTO>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<
        IReadOnlyCollection<AttachmentPrivateDTO>>> GetReceived(
        int participantId)
    {
        var files =
            await _privateAttachmentService.GetReceivedFilesAsync(
                participantId);

        return Ok(files);
    }

    /// <summary>
    /// Повертає приватні файли,
    /// надіслані вказаним учасником.
    /// </summary>
    /// <param name="participantId">
    /// Ідентифікатор учасника-відправника.
    /// </param>
    /// <returns>
    /// Колекція приватних файлів, надісланих учасником.
    /// </returns>
    /// <response code="200">
    /// Список надісланих приватних файлів успішно сформовано.
    /// </response>
    [HttpGet("sent")]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<AttachmentPrivateDTO>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<
        IReadOnlyCollection<AttachmentPrivateDTO>>> GetSent(
        int participantId)
    {
        var files =
            await _privateAttachmentService.GetSentFilesAsync(
                participantId);

        return Ok(files);
    }

    /// <summary>
    /// Повертає інформацію
    /// про конкретний приватний файл.
    /// </summary>
    /// <param name="participantId">
    /// Ідентифікатор учасника,
    /// який переглядає файл.
    /// </param>
    /// <param name="fileId">
    /// Ідентифікатор приватного файлу.
    /// </param>
    /// <response code="200">Інформацію про файл отримано.</response>
    /// <response code="404">
    /// Файл не знайдено або учасник не має доступу.
    /// </response>
    [HttpGet("{fileId:int}")]
    [ProducesResponseType(
        typeof(AttachmentPrivateDTO),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AttachmentPrivateDTO>> GetById(
        int participantId,
        int fileId)
    {
        var privateFile =
            await _privateAttachmentService.GetByIdAsync(
                fileId,
                participantId);

        if (privateFile is null)
        {
            return NotFound(new
            {
                message =
                    $"Приватний файл з ідентифікатором {fileId} " +
                    "не знайдено або доступ заборонено."
            });
        }

        return Ok(privateFile);
    }

    /// <summary>
    /// Завантажує приватний файл.
    /// </summary>
    /// <param name="participantId">
    /// Ідентифікатор учасника,
    /// який завантажує файл.
    /// </param>
    /// <param name="fileId">
    /// Ідентифікатор приватного файлу.
    /// </param>
    /// <response code="200">Файл успішно отримано.</response>
    /// <response code="404">
    /// Файл не знайдено або учасник не має доступу.
    /// </response>
    [HttpGet("{fileId:int}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(
        int participantId,
        int fileId)
    {
        var document =
            await _privateAttachmentService.DownloadAsync(
                fileId,
                participantId);

        if (document is null)
        {
            return NotFound(new
            {
                message =
                    $"Приватний файл з ідентифікатором {fileId} " +
                    "не знайдено або доступ заборонено."
            });
        }

        return File(
            document.Content,
            document.ContentType,
            document.OriginalFileName);
    }

    /// <summary>
    /// Видаляє приватний файл.
    /// </summary>
    /// <param name="participantId">
    /// Ідентифікатор учасника-відправника.
    /// </param>
    /// <param name="fileId">
    /// Ідентифікатор приватного файлу.
    /// </param>
    /// <response code="204">Файл успішно видалено.</response>
    /// <response code="404">
    /// Файл не знайдено або учасник не має права його видаляти.
    /// </response>
    [HttpDelete("{fileId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int participantId,
        int fileId)
    {
        var deleted =
            await _privateAttachmentService.DeleteAsync(
                fileId,
                participantId);

        if (!deleted)
        {
            return NotFound(new
            {
                message =
                    $"Приватний файл з ідентифікатором {fileId} " +
                    "не знайдено або видалення заборонено."
            });
        }

        return NoContent();
    }
}