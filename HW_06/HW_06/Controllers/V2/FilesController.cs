using Asp.Versioning;
using HW_06.DTOs.Files;
using HW_06.Models.Files;
using HW_06.Services.Interfaces;
using HW_06.Validators.FileValid;
using Microsoft.AspNetCore.Mvc;

namespace HW_06.Controllers;

/// <summary>
/// Контролер для роботи з файлами.
/// Дозволяє завантажувати, замінювати та видаляти аватари.
/// </summary>
[ApiVersion("2.0")]
[ApiController]
[Route("api/v{version:apiVersion}/files")]
public class FilesController : ControllerBase
{
    /// <summary>
    /// Максимально допустимий розмір аватара — 10 МБ.
    /// </summary>
    private const long MaxAvatarSize = 10 * 1024 * 1024;

    /// <summary>
    /// Назва каталогу, у якому зберігаються аватари.
    /// </summary>
    private const string AvatarFolder = "Avatars";

    private readonly IFileStorageService _fileStorage;

    /// <summary>
    /// Ініціалізує контролер для роботи з файлами.
    /// </summary>
    /// <param name="fileStorage">
    /// Сервіс локального зберігання файлів.
    /// </param>
    public FilesController(
        IFileStorageService fileStorage)
    {
        ArgumentNullException.ThrowIfNull(fileStorage);

        _fileStorage = fileStorage;
    }

    /// <summary>
    /// Завантажує новий демонстраційний аватар.
    /// </summary>
    /// <param name="dto">
    /// DTO, що містить файл аватара.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
    /// <returns>
    /// Інформація про збережений файл та URL для його перегляду.
    /// </returns>
    [HttpPost("demo-avatar")]
    [Consumes("multipart/form-data")]
    [Produces("application/json")]
    [RequestSizeLimit(MaxAvatarSize)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    public async Task<IActionResult> UploadDemoAvatar(
        [FromForm] AvatarUploadDTO dto,
        CancellationToken cancellationToken)
    {
        var validationError =
            AvatarFileValidator.ValidateAvatar(
                dto.File,
                MaxAvatarSize);

        if (validationError is not null)
        {
            return BadRequest(new
            {
                error = validationError
            });
        }

        try
        {
            var storedFileName =
                await _fileStorage.SaveAsync(
                    dto.File,
                    AvatarFolder,
                    FileAccessLevel.Public,
                    cancellationToken);

            var url = BuildAvatarUrl(
                storedFileName);

            return Ok(new
            {
                originalFileName = dto.File.FileName,
                storedFileName,
                dto.File.Length,
                dto.File.ContentType,
                url
            });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                error = exception.Message
            });
        }
    }

    /// <summary>
/// Замінює існуючий аватар новим файлом.
/// Пошук виконується за серверним ім’ям без розширення.
/// </summary>
/// <remarks>
/// Розширення існуючого файлу визначається автоматично.
/// Новий файл може мати інший формат.
/// </remarks>
/// <param name="baseFileName">
/// Серверне ім’я файлу без розширення.
/// </param>
/// <param name="dto">
/// DTO, що містить новий файл аватара.
/// </param>
/// <param name="cancellationToken">
/// Токен скасування операції.
/// </param>
/// <returns>
/// Інформація про оновлений файл та актуальний URL.
/// </returns>
[HttpPut("avatar/{baseFileName}")]
[Consumes("multipart/form-data")]
[Produces("application/json")]
[RequestSizeLimit(MaxAvatarSize)]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
public async Task<IActionResult> ReplaceAvatar(
    [FromRoute] string baseFileName,
    [FromForm] AvatarUploadDTO dto,
    CancellationToken cancellationToken)
{
    if (string.IsNullOrWhiteSpace(baseFileName))
    {
        return BadRequest(new
        {
            error = "Ім’я файлу не вказано."
        });
    }

    var validationError =
        AvatarFileValidator.ValidateAvatar(
            dto.File,
            MaxAvatarSize);

    if (validationError is not null)
    {
        return BadRequest(new
        {
            error = validationError
        });
    }

    try
    {
        var storedFileName =
            await _fileStorage.ReplaceAsync(
                dto.File,
                AvatarFolder,
                baseFileName,
                FileAccessLevel.Public,
                cancellationToken);

        var url = BuildAvatarUrl(
            storedFileName);

        return Ok(new
        {
            searchedFileName = baseFileName,
            storedFileName,
            originalFileName = dto.File.FileName,
            dto.File.Length,
            dto.File.ContentType,
            url
        });
    }
    catch (FileNotFoundException)
    {
        return NotFound(new
        {
            error =
                $"Файл з ім’ям '{baseFileName}' не знайдено."
        });
    }
    catch (ArgumentException exception)
    {
        return BadRequest(new
        {
            error = exception.Message
        });
    }
    catch (IOException exception)
    {
        return Conflict(new
        {
            error = exception.Message
        });
    }
}

    /// <summary>
    /// Видаляє аватар за його серверним ім’ям.
    /// </summary>
    /// <param name="fileName">
    /// Повне серверне ім’я файлу разом із розширенням.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
    /// <returns>
    /// Код 204 після виконання операції видалення.
    /// </returns>
    [HttpDelete("avatar/{fileName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteAvatar(
        [FromRoute] string fileName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return BadRequest(new
            {
                error = "Ім'я файлу не вказано."
            });
        }

        try
        {
            await _fileStorage.DeleteAsync(
                AvatarFolder,
                fileName,
                FileAccessLevel.Public,
                cancellationToken);

            return NoContent();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                error = exception.Message
            });
        }
    }

    /// <summary>
    /// Формує публічний URL для перегляду аватара.
    /// </summary>
    /// <param name="fileName">
    /// Серверне ім’я файлу разом із розширенням.
    /// </param>
    /// <returns>
    /// Абсолютний URL до публічного файла.
    /// </returns>
    private string BuildAvatarUrl(
        string fileName)
    {
        return
            $"{Request.Scheme}://{Request.Host}" +
            $"/uploads/{AvatarFolder}/{fileName}";
    }
    
    /// <summary>
    /// Повертає аватар за його серверним ім’ям без розширення.
    /// </summary>
    /// <remarks>
    /// Розширення файлу визначається автоматично.
    /// Підтримуються формати JPG, JPEG, PNG та WEBP.
    /// </remarks>
    /// <param name="baseFileName">
    /// Серверне ім’я файлу без розширення.
    /// </param>
    /// <returns>
    /// Файл аватара з відповідним MIME-типом.
    /// </returns>
    [HttpGet("avatar/{baseFileName}")]
    [Produces("image/jpeg", "image/png", "image/webp")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetAvatar(
        [FromRoute] string baseFileName)
    {
        if (string.IsNullOrWhiteSpace(baseFileName))
        {
            return BadRequest(new
            {
                error = "Ім’я файлу не вказано."
            });
        }

        try
        {
            var fileResult = _fileStorage.OpenRead(
                AvatarFolder,
                baseFileName,
                FileAccessLevel.Public);

            if (fileResult is null)
            {
                return NotFound(new
                {
                    error =
                        $"Файл з ім’ям '{baseFileName}' не знайдено."
                });
            }

            return File(
                fileResult.Content,
                fileResult.ContentType);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                error = exception.Message
            });
        }
    }
}