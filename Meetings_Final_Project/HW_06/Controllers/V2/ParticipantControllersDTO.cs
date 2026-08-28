using Asp.Versioning;
using HW_06.Common.Constants;
using HW_06.DTOs.Files;
using HW_06.DTOs.MeetingDTO;
using HW_06.DTOs.ParticipantDTO;
using HW_06.DTOs.ParticipantsDTO;
using HW_06.Features.Participants.Commands.Delete;
using HW_06.Features.Participants.Commands.DeleteMany;
using HW_06.Features.Participants.Commands.PartialUpdate;
using HW_06.Features.Participants.Commands.ResetAvatar;
using HW_06.Features.Participants.Commands.Update;
using HW_06.Features.Participants.Commands.UploadAvatar;
using HW_06.Features.Participants.Queries.GetAll;
using HW_06.Features.Participants.Queries.GetAvatar;
using HW_06.Features.Participants.Queries.GetById;
using HW_06.Features.Participants.Queries.GetMeetings;
using HW_06.Helpers.Pagination;
using HW_06.Helpers.QueryParameters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    private readonly ISender _sender;

    /// <summary>
    /// Максимально допустимий розмір аватара — 10 МБ.
    /// </summary>
    private const long MaxAvatarSize = 10 * 1024 * 1024;

    public ParticipantControllersDTO(ISender sender)
    {
        ArgumentNullException.ThrowIfNull(sender);
        _sender = sender;
    }

    /// <summary>
    /// Повертає власний або стандартний
    /// аватар учасника.
    /// </summary>
    [Authorize]
    [HttpGet("{participantId:int}/avatar/file")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAvatarFileAsync(int participantId, CancellationToken cancellationToken)
    {
        var file = await _sender.Send(new GetParticipantAvatarQuery(participantId), cancellationToken);

        if (file is null)
        {
            return NotFound(new
            {
                message = "Файл аватара не знайдено."
            });
        }

        return File(file.Content, file.ContentType);
    }

    /// <summary>
    /// Видаляє власний аватар учасника
    /// та повертає використання стандартного аватара.
    /// </summary>
    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpDelete("{participantId:int}/avatar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetAvatar(int participantId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ResetParticipantAvatarCommand(participantId), cancellationToken);

        if (!result)
        {
            return NotFound(new
            {
                message = $"Учасника з ідентифікатором {participantId} не знайдено."
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Отримує список учасників.
    /// Підтримує пошук, сортування та пагінацію.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<ParticipantReadDTO>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ParticipantReadDTO>>> GetParticipants(
            [FromQuery] ParticipantQueryParameters parameters, CancellationToken cancellationToken)
    {
        var participants = await _sender.Send(new GetAllParticipantsQuery(parameters),
                cancellationToken);

        return Ok(participants);
    }

    /// <summary>
    /// Отримує детальну інформацію
    /// про учасника.
    /// </summary>
    [Authorize]
    [HttpGet("{id:int}")]
    [ProducesResponseType<ParticipantDetailDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParticipantDetailDTO>> GetById(int id, CancellationToken cancellationToken)
    {
        var participant = await _sender.Send(new GetParticipantByIdQuery(id), cancellationToken);

        if (participant is null)
        {
            return NotFound(new
            {
                message = "Учасника із зазначеним ідентифікатором не знайдено."
            });
        }

        return Ok(participant);
    }

    /// <summary>
    /// Встановлює або замінює
    /// аватар учасника.
    /// </summary>
    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPut("{participantId:int}/avatar")]
    [Consumes("multipart/form-data")]
    [Produces("application/json")]
    [RequestSizeLimit(MaxAvatarSize)]
    [ProducesResponseType<ParticipantAvatarDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    public async Task<ActionResult<ParticipantAvatarDTO>> UploadAvatarAsync(int participantId,
            [FromForm] AvatarUploadDTO dto, CancellationToken cancellationToken)
    {
        var participantAvatar = await _sender.Send(new UploadParticipantAvatarCommand(participantId, dto.File),
                cancellationToken);

        if (participantAvatar is null)
        {
            return NotFound(new
            {
                message = $"Учасника з ідентифікатором {participantId} не знайдено."
            });
        }

        return Ok(AddAvatarUrl(participantAvatar));
    }

    /// <summary>
    /// Повне оновлення інформації
    /// про учасника.
    /// </summary>
    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] ParticipantUpdateDTO dto,
        CancellationToken cancellationToken)
    {
        var updated = await _sender.Send(new UpdateParticipantCommand(id, dto),
                cancellationToken);

        if (!updated)
        {
            return NotFound(new
            {
                message = $"Учасника з ідентифікатором {id} не знайдено."
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Часткове оновлення інформації
    /// про учасника.
    /// </summary>
    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PartialUpdate(int id, [FromBody] ParticipantPartialUpdateDTO dto,
        CancellationToken cancellationToken)
    {
        var updated = await _sender.Send(new PartialUpdateParticipantCommand(id, dto),
                cancellationToken);

        if (!updated)
        {
            return NotFound(new
            {
                message = $"Учасника з ідентифікатором {id} не знайдено."
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Видаляє учасника.
    /// </summary>
    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _sender.Send(new DeleteParticipantCommand(id), cancellationToken);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Учасника із зазначеним ідентифікатором не знайдено."
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Видаляє декількох учасників.
    /// </summary>
    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpDelete("delete-many")]
    [ProducesResponseType<int>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> DeleteMany([FromBody] List<int> ids, CancellationToken cancellationToken)
    {
        var deletedCount = await _sender.Send(new DeleteManyParticipantsCommand(ids), cancellationToken);

        return Ok(deletedCount);
    }

    /// <summary>
    /// Отримує список зустрічей,
    /// у яких бере участь вказаний учасник.
    /// </summary>
    [Authorize]
    [HttpGet("{id:int}/meetings")]
    [ProducesResponseType<List<MeetingReadDTO>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<MeetingReadDTO>>> GetMeetings(int id, CancellationToken cancellationToken)
    {
        var meetings = await _sender.Send(new GetParticipantMeetingsQuery(id),
                cancellationToken);

        return Ok(meetings);
    }

    /// <summary>
    /// Додає до DTO абсолютний URL
    /// для отримання аватара.
    /// </summary>
    private ParticipantAvatarDTO AddAvatarUrl(ParticipantAvatarDTO participantAvatar)
    {
        var avatarUrl = Url.Action(action: nameof(GetAvatarFileAsync), controller: null, values: new
                {
                    version = "2.0",
                    participantId = participantAvatar.ParticipantId
                },
                protocol: Request.Scheme);

        return participantAvatar with
        {
            AvatarUrl = avatarUrl
        };
    }
}