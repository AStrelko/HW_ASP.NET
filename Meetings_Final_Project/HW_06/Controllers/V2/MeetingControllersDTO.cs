using System.Diagnostics;
using System.Security.Claims;
using Asp.Versioning;
using HW_06.Common.Constants;
using HW_06.DTOs.MeetingDTO;
using HW_06.Features.Meetings.Commands.Create;
using HW_06.Features.Meetings.Commands.Delete;
using HW_06.Features.Meetings.Commands.DeleteMany;
using HW_06.Features.Meetings.Commands.PartialUpdate;
using HW_06.Features.Meetings.Commands.Update;
using HW_06.Features.Meetings.Queries.GetAll;
using HW_06.Features.Meetings.Queries.GetById;
using HW_06.Features.Meetings.Queries.GetByParticipant;
using HW_06.Helpers.Pagination;
using HW_06.Helpers.QueryParameters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace HW_06.Controllers;

/// <summary>
/// Контролер для керування зустрічами.
/// Підтримує отримання, створення, оновлення,
/// видалення, пошук, сортування та пагінацію.
/// </summary>
[ApiVersion("2.0")]
[ApiController]
[Route("api/v{version:apiVersion}/meetings")]
[Consumes("application/json")]
[ProducesResponseType<ProblemDetails>(
    StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(
    StatusCodes.Status429TooManyRequests)]
[ProducesResponseType<ProblemDetails>(
    StatusCodes.Status500InternalServerError)]
public class MeetingControllersDTO : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>
    /// Ініціалізує контролер зустрічей.
    /// </summary>
    /// <param name="sender">
    /// Сервіс MediatR для надсилання команд і запитів.
    /// </param>
    public MeetingControllersDTO(ISender sender)
    {
        ArgumentNullException.ThrowIfNull(sender);

        _sender = sender;
    }

    /// <summary>
    /// Отримує список зустрічей із підтримкою
    /// пошуку, сортування, фільтрації та пагінації.
    /// </summary>
    /// <param name="filter">Параметри фільтрації.</param>
    /// <param name="parameters">Параметри пошуку, сортування та пагінації.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="200">Список зустрічей отримано.</response>
    /// <response code="400">Некоректні параметри запиту.</response>
    /// <response code="401">Користувач не авторизований.</response>
    /// <response code="429">Перевищено ліміт запитів.</response>
    /// <response code="500">Внутрішня помилка сервера.</response>
    [Authorize]
    [HttpGet]
    [ProducesResponseType<PagedResult<MeetingReadDTO>>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<MeetingReadDTO>>>
        GetMeetings(
            [FromQuery] MeetingFilter filter,
            [FromQuery] MeetingQueryParameters parameters,
            CancellationToken cancellationToken)
    {
        var meetings =
            await _sender.Send(
                new GetAllMeetingsQuery(
                    filter,
                    parameters),
                cancellationToken);

        return Ok(meetings);
    }

    /// <summary>
    /// Отримує детальну інформацію
    /// про зустріч за ідентифікатором.
    /// </summary>
    /// <param name="id">Ідентифікатор зустрічі.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="200">Зустріч знайдено.</response>
    /// <response code="400">Некоректний ідентифікатор.</response>
    /// <response code="401">Користувач не авторизований.</response>
    /// <response code="404">Зустріч не знайдено.</response>
    /// <response code="429">Перевищено ліміт запитів.</response>
    /// <response code="500">Внутрішня помилка сервера.</response>
    [Authorize]
    [HttpGet("{id:int}")]
    [ProducesResponseType<MeetingDetailDTO>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MeetingDetailDTO>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var meeting =
            await _sender.Send(
                new GetMeetingByIdQuery(id),
                cancellationToken);

        if (meeting is null)
        {
            return MeetingNotFound(id);
        }

        return Ok(meeting);
    }

    /// <summary>
    /// Створює нову зустріч.
    /// Організатор визначається з JWT поточного користувача.
    /// </summary>
    /// <param name="dto">Дані нової зустрічі.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="201">Зустріч створено.</response>
    /// <response code="400">Дані не пройшли валідацію.</response>
    /// <response code="401">Користувач не авторизований.</response>
    /// <response code="429">Перевищено ліміт запитів.</response>
    /// <response code="500">Внутрішня помилка сервера.</response>
    [Authorize]
    [HttpPost]
    [ProducesResponseType<MeetingReadDTO>(
        StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MeetingReadDTO>> Create(
        [FromBody] MeetingCreateDTO dto,
        CancellationToken cancellationToken)
    {
        var organizerId =
            User.FindFirstValue(
                JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(organizerId))
        {
            return CreateProblemResponse(
                StatusCodes.Status401Unauthorized,
                "Користувач не авторизований.",
                "Не вдалося визначити організатора зустрічі. "
                + "Виконайте вхід повторно.");
        }

        var createdMeeting =
            await _sender.Send(
                new CreateMeetingCommand(
                    dto,
                    organizerId),
                cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                version = "2.0",
                id = createdMeeting.MeetingId
            },
            createdMeeting);
    }

    /// <summary>
    /// Повністю оновлює існуючу зустріч.
    /// Доступно лише адміністратору.
    /// </summary>
    /// <param name="id">Ідентифікатор зустрічі.</param>
    /// <param name="dto">Повний набір оновлених даних.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="204">Зустріч оновлено.</response>
    /// <response code="400">Дані не пройшли валідацію.</response>
    /// <response code="401">Користувач не авторизований.</response>
    /// <response code="403">Недостатньо прав.</response>
    /// <response code="404">Зустріч не знайдено.</response>
    /// <response code="429">Перевищено ліміт запитів.</response>
    /// <response code="500">Внутрішня помилка сервера.</response>
    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] MeetingUpdateDTO dto,
        CancellationToken cancellationToken)
    {
        var updated =
            await _sender.Send(
                new UpdateMeetingCommand(id, dto),
                cancellationToken);

        if (!updated)
        {
            return MeetingNotFound(id);
        }

        return NoContent();
    }

    /// <summary>
    /// Частково оновлює існуючу зустріч.
    /// Змінюються лише передані поля.
    /// Доступно лише адміністратору.
    /// </summary>
    /// <param name="id">Ідентифікатор зустрічі.</param>
    /// <param name="dto">Дані для часткового оновлення.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="204">Зустріч оновлено.</response>
    /// <response code="400">Дані не пройшли валідацію.</response>
    /// <response code="401">Користувач не авторизований.</response>
    /// <response code="403">Недостатньо прав.</response>
    /// <response code="404">Зустріч не знайдено.</response>
    /// <response code="429">Перевищено ліміт запитів.</response>
    /// <response code="500">Внутрішня помилка сервера.</response>
    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PartialUpdate(
        int id,
        [FromBody] MeetingPartialUpdateDTO dto,
        CancellationToken cancellationToken)
    {
        var updated =
            await _sender.Send(
                new PartialUpdateMeetingCommand(id, dto),
                cancellationToken);

        if (!updated)
        {
            return MeetingNotFound(id);
        }

        return NoContent();
    }

    /// <summary>
    /// Видаляє зустріч за ідентифікатором.
    /// Доступно лише адміністратору.
    /// </summary>
    /// <param name="id">Ідентифікатор зустрічі.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="204">Зустріч видалено.</response>
    /// <response code="400">Некоректний ідентифікатор.</response>
    /// <response code="401">Користувач не авторизований.</response>
    /// <response code="403">Недостатньо прав.</response>
    /// <response code="404">Зустріч не знайдено.</response>
    /// <response code="429">Перевищено ліміт запитів.</response>
    /// <response code="500">Внутрішня помилка сервера.</response>
    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted =
            await _sender.Send(
                new DeleteMeetingCommand(id),
                cancellationToken);

        if (!deleted)
        {
            return MeetingNotFound(id);
        }

        return NoContent();
    }

    /// <summary>
    /// Видаляє декілька зустрічей за списком ідентифікаторів.
    /// Доступно лише адміністратору.
    /// </summary>
    /// <param name="ids">Список ідентифікаторів зустрічей.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="200">Повертається кількість видалених зустрічей.</response>
    /// <response code="400">Список ідентифікаторів некоректний.</response>
    /// <response code="401">Користувач не авторизований.</response>
    /// <response code="403">Недостатньо прав.</response>
    /// <response code="429">Перевищено ліміт запитів.</response>
    /// <response code="500">Внутрішня помилка сервера.</response>
    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpDelete("delete-many")]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<int>> DeleteMany(
        [FromBody] List<int> ids,
        CancellationToken cancellationToken)
    {
        var deletedCount =
            await _sender.Send(
                new DeleteManyMeetingsCommand(ids),
                cancellationToken);

        return Ok(deletedCount);
    }

    /// <summary>
    /// Отримує всі зустрічі,
    /// у яких бере участь зазначений учасник.
    /// </summary>
    /// <param name="participantId">Ідентифікатор учасника.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="200">Список зустрічей отримано.</response>
    /// <response code="400">Некоректний ідентифікатор учасника.</response>
    /// <response code="401">Користувач не авторизований.</response>
    /// <response code="404">Учасника не знайдено.</response>
    /// <response code="429">Перевищено ліміт запитів.</response>
    /// <response code="500">Внутрішня помилка сервера.</response>
    [Authorize]
    [HttpGet("by-participant/{participantId:int}")]
    [ProducesResponseType<IEnumerable<MeetingReadDTO>>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<MeetingReadDTO>>>
        GetByParticipant(
            int participantId,
            CancellationToken cancellationToken)
    {
        var meetings =
            await _sender.Send(
                new GetMeetingsByParticipantQuery(participantId),
                cancellationToken);

        return Ok(meetings);
    }

    /// <summary>
    /// Формує відповідь про відсутність зустрічі.
    /// </summary>
    private ObjectResult MeetingNotFound(int id)
    {
        return CreateProblemResponse(
            StatusCodes.Status404NotFound,
            "Зустріч не знайдена.",
            $"Зустріч з ідентифікатором {id} не знайдено.");
    }

    /// <summary>
    /// Формує відповідь про помилку
    /// у форматі ProblemDetails.
    /// </summary>
    private ObjectResult CreateProblemResponse(
        int statusCode,
        string title,
        string detail)
    {
        var type = statusCode switch
        {
            StatusCodes.Status401Unauthorized =>
                "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.2",

            StatusCodes.Status404NotFound =>
                "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.5",

            _ => "about:blank"
        };

        return Problem(
            detail: detail,
            instance: Request.Path,
            statusCode: statusCode,
            title: title,
            type: type);
    }
}