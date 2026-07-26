using Asp.Versioning;
using HW_06.DTOs.MeetingDTO;
using HW_06.DTOs.ParticipantDTO;
using HW_06.Helpers.Pagination;
using HW_06.Helpers.QueryParameters;
using HW_06.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using HW_06.Validators.Exceptions;

namespace HW_06.Controllers;

/// <summary>
/// Контролер для керування учасниками зустрічей.
/// Надає CRUD-операції, пошук, сортування,
/// пагінацію та отримання зустрічей учасника.
/// </summary>
[ApiVersion("2.0")]
[ApiController]
[Route("api/v{version:apiVersion}/participants")]
[Consumes("application/json")]
[Produces("application/json")]
public class ParticipantControllersDTO : ControllerBase
{
    private readonly IParticipantService _service;

    public ParticipantControllersDTO(IParticipantService service)
    {
        _service = service;
    }

    /// <summary>
    /// Отримання списку учасників.
    /// Підтримує пошук, сортування та пагінацію.
    /// </summary>
    /// <param name="parameters">
    /// Параметри пошуку, сортування та пагінації.
    /// </param>
    /// <returns>Сторінка зі списком учасників.</returns>
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
    /// Створення нового учасника.
    /// </summary>
    /// <param name="dto">
    /// Дані нового учасника.
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
    [ProducesResponseType<ParticipantReadDTO>(
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ParticipantReadDTO>> Create(
        [FromBody] ParticipantCreateDTO dto)
    {
        try
        {
            var createdParticipant =
                await _service.CreateAsync(dto);

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
    /// Повне оновлення інформації про учасника.
    /// </summary>
    /// <param name="id">Ідентифікатор учасника.</param>
    /// <param name="dto">Нові дані учасника.</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] ParticipantUpdateDTO dto)
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
}