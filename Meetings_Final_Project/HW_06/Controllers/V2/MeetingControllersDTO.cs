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
/// Надає операції отримання, створення, оновлення,
/// видалення, пошуку, сортування, фільтрації
/// та пагінації зустрічей.
/// </summary>
[ApiVersion("2.0")]
[ApiController]
[Route("api/v{version:apiVersion}/meetings")]
[Consumes("application/json")]
[Produces("application/json")]
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
    /// Отримує список зустрічей
    /// із підтримкою пошуку, сортування,
    /// фільтрації та пагінації.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<MeetingReadDTO>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<MeetingReadDTO>>>
        GetMeetings([FromQuery] MeetingFilter filter, [FromQuery] MeetingQueryParameters parameters,
            CancellationToken cancellationToken)
    {
        var meetings = await _sender.Send(
            new GetAllMeetingsQuery(filter, parameters), cancellationToken);

        return Ok(meetings);
    }

    /// <summary>
    /// Отримує детальну інформацію
    /// про зустріч за ідентифікатором.
    /// </summary>
    [Authorize]
    [HttpGet("{id:int}")]
    [ProducesResponseType<MeetingDetailDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MeetingDetailDTO>> GetById(int id, CancellationToken cancellationToken)
    {
        var meeting = await _sender.Send(new GetMeetingByIdQuery(id), cancellationToken);

        if (meeting is null)
        {
            return NotFound(new
            {
                message = $"Зустріч з ідентифікатором {id} не знайдено."
            });
        }

        return Ok(meeting);
    }

    /// <summary>
    /// Створює нову зустріч.
    /// </summary>
    [Authorize]
    [HttpPost]
    [ProducesResponseType<MeetingReadDTO>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MeetingReadDTO>>
        Create([FromBody] MeetingCreateDTO dto, CancellationToken cancellationToken)
    {
        var organizerId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(organizerId))
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Користувач не авторизований.",
                Detail = "Для створення зустрічі необхідно авторизуватися."
            });
        }

        var createdMeeting = await _sender.Send( new CreateMeetingCommand(dto, organizerId), cancellationToken);

        return CreatedAtAction(nameof(GetById), new
            {
                version = "2.0",
                id = createdMeeting.MeetingId
            }, createdMeeting);
    }

    /// <summary>
    /// Повністю оновлює існуючу зустріч.
    /// </summary>
    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] MeetingUpdateDTO dto,
        CancellationToken cancellationToken)
    {
        var updated = await _sender.Send(new UpdateMeetingCommand(id, dto), cancellationToken);

        if (!updated)
        {
            return NotFound(new
            {
                message = $"Зустріч з ідентифікатором {id} не знайдено."
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Частково оновлює існуючу зустріч.
    /// Змінюються лише передані поля.
    /// </summary>
    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PartialUpdate(int id, [FromBody] MeetingPartialUpdateDTO dto,
        CancellationToken cancellationToken)
    {
        var updated = await _sender.Send(new PartialUpdateMeetingCommand(id, dto), cancellationToken);

        if (!updated)
        {
            return NotFound(new
            {
                message = $"Зустріч з ідентифікатором {id} не знайдено."
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Видаляє зустріч за ідентифікатором.
    /// </summary>
    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _sender.Send(new DeleteMeetingCommand(id), cancellationToken);

        if (!deleted)
        {
            return NotFound(new
            {
                message = $"Зустріч з ідентифікатором {id} не знайдено."
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Видаляє декілька зустрічей
    /// за списком ідентифікаторів.
    /// </summary>
    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpDelete("delete-many")]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> DeleteMany([FromBody] List<int> ids, CancellationToken cancellationToken)
    {
        var deletedCount = await _sender.Send(new DeleteManyMeetingsCommand(ids), cancellationToken);

        return Ok(deletedCount);
    }

    /// <summary>
    /// Отримує всі зустрічі,
    /// у яких бере участь зазначений учасник.
    /// </summary>
    [Authorize]
    [HttpGet("by-participant/{participantId:int}")]
    [ProducesResponseType<IEnumerable<MeetingReadDTO>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<MeetingReadDTO>>> GetByParticipant(int participantId,
            CancellationToken cancellationToken)
    {
        var meetings = await _sender.Send(new GetMeetingsByParticipantQuery(participantId),
                cancellationToken);

        return Ok(meetings);
    }
}