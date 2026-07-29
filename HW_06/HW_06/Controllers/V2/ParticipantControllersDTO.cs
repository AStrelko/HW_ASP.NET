using Asp.Versioning;
using HW_06.DTOs.MeetingDTO;
using HW_06.DTOs.ParticipantDTO;
using HW_06.DTOs.Participants;
using HW_06.Helpers.Pagination;
using HW_06.Helpers.QueryParameters;
using HW_06.Services.Interfaces;
using HW_06.Validators.Exceptions;
using Microsoft.AspNetCore.Mvc;
using HW_06.DTOs.Files;
using HW_06.Models.Files;
using HW_06.Validators.FileValid;

namespace HW_06.Controllers;

/// <summary>
/// Контролер для керування учасниками зустрічей.
/// Надає CRUD-операції, пошук, сортування,
/// пагінацію та роботу з аватарами учасників.
/// </summary>
[ApiVersion("2.0")]
[ApiController]
[Route("api/v{version:apiVersion}/participants")]
[Consumes("application/json")]
[Produces("application/json")]
public class ParticipantControllersDTO : ControllerBase
{
    /// <summary>
    /// Ініціалізує контролер учасників.
    /// </summary>
    /// <param name="service">
    /// Сервіс для роботи з учасниками.
    /// </param>
    /// <param name="fileStorageService">
    /// Сервіс локального файлового сховища.
    /// </param>
    public ParticipantControllersDTO(
        IParticipantService service,
        IFileStorageService fileStorageService)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(fileStorageService);

        _service = service;
        _fileStorageService = fileStorageService;
    }
    /// <summary>
    /// Максимально допустимий розмір аватара — 10 МБ.
    /// </summary>
    private const long MaxAvatarSize =
        10 * 1024 * 1024;

    /// <summary>
    /// Каталог зберігання аватарів.
    /// </summary>
    private const string AvatarFolder =
        "Avatars";
    
    /// <summary>
    /// Базове ім’я стандартного аватара без розширення.
    /// </summary>
    private const string DefaultAvatarFileName =
        "default";

    private readonly IParticipantService _service;
    private readonly IFileStorageService _fileStorageService;

    /// <summary>
    /// Повертає власний або стандартний аватар учасника.
    /// </summary>
    /// <param name="participantId">
    /// Унікальний ідентифікатор учасника.
    /// </param>
    /// <returns>
    /// Власний аватар учасника або стандартне зображення,
    /// якщо власний аватар відсутній.
    /// </returns>
    /// <response code="200">
    /// Файл аватара успішно отримано.
    /// </response>
    /// <response code="404">
    /// Учасника із зазначеним ідентифікатором не знайдено.
    /// </response>
    [HttpGet("{participantId:int}/avatar/file")]
    [Produces(
        "image/jpeg",
        "image/png",
        "image/webp",
        "application/octet-stream")]
    [ProducesResponseType(
        typeof(FileResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAvatarFileAsync(
        int participantId)
    {
        var participantAvatar =
            await _service.GetAvatarAsync(participantId);

        if (participantAvatar is null)
        {
            return NotFound();
        }

        var avatarBaseFileName =
            string.IsNullOrWhiteSpace(
                participantAvatar.AvatarFileName)
                ? DefaultAvatarFileName
                : participantAvatar.AvatarFileName;

        var fileResult =
            _fileStorageService.OpenRead(
                AvatarFolder,
                avatarBaseFileName,
                FileAccessLevel.Public);

        if (fileResult is null &&
            avatarBaseFileName != DefaultAvatarFileName)
        {
            fileResult =
                _fileStorageService.OpenRead(
                    AvatarFolder,
                    DefaultAvatarFileName,
                    FileAccessLevel.Public);
        }

        if (fileResult is null)
        {
            return NotFound();
        }

        return File(
            fileResult.Content,
            fileResult.ContentType);
    }

    /// <summary>
    /// Отримання списку учасників.
    /// Підтримує пошук, сортування та пагінацію.
    /// </summary>
    /// <param name="parameters">
    /// Параметри пошуку, сортування та пагінації.
    /// </param>
    /// <returns>
    /// Сторінка зі списком учасників.
    /// </returns>
    [HttpGet]
    [ProducesResponseType<PagedResult<ParticipantReadDTO>>(
        StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ParticipantReadDTO>>>
        GetParticipants(
            [FromQuery] ParticipantQueryParameters parameters)
    {
        var participants =
            await _service.GetAllAsync(parameters);

        return Ok(participants);
    }

    /// <summary>
    /// Отримання детальної інформації про учасника.
    /// </summary>
    /// <param name="id">
    /// Ідентифікатор учасника.
    /// </param>
    /// <returns>
    /// Повна інформація про учасника та його зустрічі.
    /// </returns>
    /// <response code="200">
    /// Інформацію про учасника успішно отримано.
    /// </response>
    /// <response code="404">
    /// Учасника із зазначеним ідентифікатором не знайдено.
    /// </response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<ParticipantDetailDTO>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParticipantDetailDTO>>
        GetById(int id)
    {
        var participant =
            await _service.GetByIdAsync(id);

        if (participant is null)
        {
            return NotFound(new
            {
                message =
                    "Учасника із зазначеним ідентифікатором не знайдено."
            });
        }

        return Ok(participant);
    }

    /// <summary>
    /// Створює нового учасника та, за наявності,
    /// завантажує його аватар.
    /// </summary>
    /// <param name="dto">
    /// Дані нового учасника та необов’язковий файл аватара.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
    /// <returns>
    /// Створений учасник.
    /// </returns>
    /// <response code="201">
    /// Учасника успішно створено.
    /// </response>
    /// <response code="400">
    /// Передані дані не пройшли перевірку.
    /// </response>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<ParticipantReadDTO>(
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ParticipantReadDTO>> Create(
        [FromForm] ParticipantCreateDTO dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var createdParticipant =
                await _service.CreateAsync(
                    dto,
                    cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    version = "2.0",
                    id = createdParticipant.ParticipantId
                },
                createdParticipant);
        }
        catch (ValidationException exception)
        {
            return BadRequest(new
            {
                message =
                    "Не вдалося створити учасника.",
                errors = exception.Errors
            });
        }
    }
    
 /// <summary>
/// Додає або замінює аватар учасника.
/// </summary>
/// <remarks>
/// Якщо аватар відсутній — буде додано новий.
/// Якщо аватар уже існує — він буде автоматично замінений.
/// </remarks>
/// <param name="participantId">
/// Унікальний ідентифікатор учасника.
/// </param>
/// <param name="dto">
/// DTO, що містить файл аватара.
/// </param>
/// <param name="cancellationToken">
/// Токен скасування операції.
/// </param>
/// <returns>
/// Інформація про учасника та URL аватара.
/// </returns>
/// <response code="200">
/// Аватар успішно збережено.
/// </response>
/// <response code="400">
/// Файл не пройшов перевірку.
/// </response>
/// <response code="404">
/// Учасника із зазначеним ідентифікатором не знайдено.
/// </response>
/// <response code="413">
/// Розмір файла перевищує допустиме значення.
/// </response>
[HttpPut("{participantId:int}/avatar")]
[Consumes("multipart/form-data")]
[Produces("application/json")]
[RequestSizeLimit(MaxAvatarSize)]
[ProducesResponseType<ParticipantAvatarDTO>(
    StatusCodes.Status200OK)]
[ProducesResponseType(
    StatusCodes.Status400BadRequest)]
[ProducesResponseType(
    StatusCodes.Status404NotFound)]
[ProducesResponseType(
    StatusCodes.Status413PayloadTooLarge)]
public async Task<ActionResult<ParticipantAvatarDTO>>
    UploadAvatarAsync(
        int participantId,
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
            message = validationError
        });
    }

    try
    {
        var participantAvatar =
            await _service.UploadAvatarAsync(
                participantId,
                dto.File,
                cancellationToken);

        if (participantAvatar is null)
        {
            return NotFound(new
            {
                message =
                    "Учасника із зазначеним " +
                    "ідентифікатором не знайдено."
            });
        }

        var result =
            AddAvatarUrl(participantAvatar);

        return Ok(result);
    }
    catch (ValidationException exception)
    {
        return BadRequest(new
        {
            message =
                "Не вдалося зберегти аватар.",
            errors = exception.Errors
        });
    }
    catch (ArgumentException exception)
    {
        return BadRequest(new
        {
            message = exception.Message
        });
    }
}
    /// <summary>
    /// Повне оновлення інформації про учасника.
    /// </summary>
    /// <param name="id">Ідентифікатор учасника.</param>
    /// <param name="dto">Нові дані учасника.</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id,[FromBody] ParticipantUpdateDTO dto)
    {
        if (id != dto.ParticipantId)
        {
            return BadRequest(new
            {
                message =
                    "Ідентифікатор у маршруті не збігається " +
                    "з ідентифікатором у тілі запиту."
            });
        }

        try
        {
            var updated =
                await _service.UpdateAsync(id, dto);

            if (!updated)
            {
                return NotFound(new
                {
                    message =
                        "Учасника із зазначеним ідентифікатором не знайдено."
                });
            }

            return NoContent();
        }
        catch (ValidationException exception)
        {
            return BadRequest(new
            {
                message =
                    "Не вдалося оновити учасника.",
                errors = exception.Errors
            });
        }
    }

    /// <summary>
    /// Часткове оновлення інформації про учасника.
    /// </summary>
    /// <param name="id">Ідентифікатор учасника.</param>
    /// <param name="dto">Поля, які необхідно оновити.</param>
    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PartialUpdate(
        int id,
        [FromBody] ParticipantPartialUpdateDTO dto)
    {
        try
        {
            var updated =
                await _service.PartialUpdateAsync(id, dto);

            if (!updated)
            {
                return NotFound(new
                {
                    message =
                        "Учасника із зазначеним ідентифікатором не знайдено."
                });
            }

            return NoContent();
        }
        catch (ValidationException exception)
        {
            return BadRequest(new
            {
                message =
                    "Не вдалося частково оновити учасника.",
                errors = exception.Errors
            });
        }
    }

    /// <summary>
    /// Видалення учасника.
    /// </summary>
    /// <param name="id">Ідентифікатор учасника.</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted =
            await _service.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message =
                    "Учасника із зазначеним ідентифікатором не знайдено."
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Видалення декількох учасників.
    /// </summary>
    /// <param name="ids">Список ідентифікаторів учасників.</param>
    /// <returns>Кількість видалених учасників.</returns>
    [HttpDelete("delete-many")]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> DeleteMany(
        [FromBody] List<int>? ids)
    {
        if (ids is null || ids.Count == 0)
        {
            return BadRequest(new
            {
                message =
                    "Список ідентифікаторів порожній."
            });
        }

        if (ids.Any(id => id <= 0))
        {
            return BadRequest(new
            {
                message =
                    "Усі ідентифікатори повинні бути більшими за нуль."
            });
        }

        var deletedCount =
            await _service.DeleteManyAsync(ids);

        return Ok(deletedCount);
    }
    /// <summary>
    /// Отримання зустрічей, у яких бере участь
    /// вказаний учасник.
    /// </summary>
    /// <param name="id">Ідентифікатор учасника.</param>
    /// <returns>Список зустрічей учасника.</returns>
    [HttpGet("{id:int}/meetings")]
    [ProducesResponseType<List<MeetingReadDTO>>(
        StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MeetingReadDTO>>>
        GetMeetings(int id)
    {
        var meetings =
            await _service.GetMeetingsAsync(id);

        return Ok(meetings);
    }
    
    /// <summary>
    /// Додає до DTO абсолютний URL для отримання аватара.
    /// </summary>
    /// <param name="participantAvatar">
    /// DTO з інформацією про аватар учасника.
    /// </param>
    /// <returns>
    /// DTO з абсолютним URL власного або стандартного аватара.
    /// </returns>
    private ParticipantAvatarDTO AddAvatarUrl(
        ParticipantAvatarDTO participantAvatar)
    {
        var avatarUrl = Url.Action(
            action: nameof(GetAvatarFileAsync),
            controller: null,
            values: new
            {
                version = "2.0",
                participantId =
                    participantAvatar.ParticipantId
            },
            protocol: Request.Scheme);

        return participantAvatar with
        {
            AvatarUrl = avatarUrl
        };
    }
}