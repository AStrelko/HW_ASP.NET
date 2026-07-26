using Asp.Versioning;
using HW_06.DTOs.MeetingDTO;
using HW_06.Helpers.Pagination;
using HW_06.Helpers.QueryParameters;
using HW_06.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using HW_06.Validators.Exceptions;

namespace HW_06.Controllers;

/// <summary>
/// Контролер для керування зустрічами.
/// Надає CRUD-операції, пошук, сортування, фільтрацію,
/// пагінацію та отримання зустрічей за учасником.
/// </summary>
[ApiVersion("2.0")]
[ApiController]
[Route("api/v{version:apiVersion}/meetings")]
[Consumes("application/json")]
[Produces("application/json")]
public class MeetingControllersDTO : ControllerBase
{
    private readonly IMeetingService _service;

    public MeetingControllersDTO(IMeetingService service)
    {
        _service = service;
    }

    /// <summary>
    /// Отримання списку зустрічей.
    /// Підтримує пошук, сортування, фільтрацію та пагінацію.
    /// </summary>
    /// <param name="filter">Параметри фільтрації зустрічей.</param>
    /// <param name="parameters">
    /// Параметри пошуку, сортування та пагінації.
    /// </param>
    /// <returns>Сторінка зі списком зустрічей.</returns>
    [HttpGet]
    [ProducesResponseType<PagedResult<MeetingReadDTO>>(
        StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<MeetingReadDTO>>> GetMeetings(
        [FromQuery] MeetingFilter filter,
        [FromQuery] MeetingQueryParameters parameters)
    {
        var meetings = await _service.GetAllAsync(filter, parameters);

        return Ok(meetings);
    }

    /// <summary>
    /// Отримання детальної інформації про зустріч.
    /// </summary>
    /// <param name="id">Ідентифікатор зустрічі.</param>
    /// <returns>Повна інформація про зустріч.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType<MeetingDetailDTO>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MeetingDetailDTO>> GetById(int id)
    {
        var meeting = await _service.GetByIdAsync(id);

        if (meeting is null)
            return NotFound();

        return Ok(meeting);
    }

    /// <summary>
    /// Створення нової зустрічі.
    /// </summary>
    /// <param name="dto">Дані нової зустрічі.</param>
    /// <returns>Створена зустріч.</returns>
    [HttpPost]
    [ProducesResponseType<MeetingReadDTO>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MeetingReadDTO>> Create(
        [FromBody] MeetingCreateDTO dto)
    {
        try
        {
            var createdMeeting =
                await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    version = "2.0",
                    id = createdMeeting.MeetingId
                },
                createdMeeting);
        }
        catch (ValidationException exception)
        {
            return BadRequest(new
            {
                message = "Не вдалося створити зустріч.",
                errors = exception.Errors
            });
        }
    }

    /// <summary>
    /// Повне оновлення зустрічі.
    /// </summary>
    /// <param name="id">
    /// Ідентифікатор зустрічі.
    /// </param>
    /// <param name="dto">
    /// Нові дані зустрічі.
    /// </param>
    /// <returns>
    /// Результат виконання операції оновлення.
    /// </returns>
    /// <response code="204">
    /// Зустріч успішно оновлено.
    /// </response>
    /// <response code="400">
    /// Передані дані не пройшли перевірку.
    /// </response>
    /// <response code="404">
    /// Зустріч із зазначеним ідентифікатором не знайдено.
    /// </response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] MeetingUpdateDTO dto)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, dto);

            if (!updated)
            {
                return NotFound(new
                {
                    message = "Зустріч із зазначеним ідентифікатором не знайдено."
                });
            }

            return NoContent();
        }
        catch (ValidationException exception)
        {
            return BadRequest(new
            {
                message = "Не вдалося оновити зустріч.",
                errors = exception.Errors
            });
        }
    }

    /// <summary>
    /// Часткове оновлення зустрічі.
    /// </summary>
    /// <param name="id">
    /// Ідентифікатор зустрічі.
    /// </param>
    /// <param name="dto">
    /// Поля зустрічі, які необхідно оновити.
    /// </param>
    /// <returns>
    /// Результат виконання операції часткового оновлення.
    /// </returns>
    /// <response code="204">
    /// Зустріч успішно оновлено.
    /// </response>
    /// <response code="400">
    /// Передані дані не пройшли перевірку.
    /// </response>
    /// <response code="404">
    /// Зустріч із зазначеним ідентифікатором не знайдено.
    /// </response>
    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PartialUpdate(
        int id,
        [FromBody] MeetingPartialUpdateDTO dto)
    {
        try
        {
            var updated =
                await _service.PartialUpdateAsync(id, dto);

            if (!updated)
            {
                return NotFound(new
                {
                    message = "Зустріч із зазначеним ідентифікатором не знайдено."
                });
            }

            return NoContent();
        }
        catch (ValidationException exception)
        {
            return BadRequest(new
            {
                message = "Не вдалося оновити зустріч.",
                errors = exception.Errors
            });
        }
    }

    /// <summary>
    /// Видалення зустрічі за ідентифікатором.
    /// </summary>
    /// <param name="id">
    /// Ідентифікатор зустрічі.
    /// </param>
    /// <returns>
    /// Результат виконання операції видалення.
    /// </returns>
    /// <response code="204">
    /// Зустріч успішно видалено.
    /// </response>
    /// <response code="404">
    /// Зустріч із зазначеним ідентифікатором не знайдено.
    /// </response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Зустріч із зазначеним ідентифікатором не знайдено."
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Видалення декількох зустрічей.
    /// </summary>
    /// <param name="ids">Список ідентифікаторів зустрічей.</param>
    /// <returns>Кількість видалених зустрічей.</returns>
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
                message = "Список ідентифікаторів порожній."
            });
        }

        var deletedCount =
            await _service.DeleteManyAsync(ids);

        return Ok(deletedCount);
    }

    /// <summary>
    /// Отримання зустрічей конкретного учасника.
    /// </summary>
    /// <param name="participantId">
    /// Ідентифікатор учасника.
    /// </param>
    /// <returns>Список зустрічей учасника.</returns>
    [HttpGet("by-participant/{participantId:int}")]
    [ProducesResponseType<IEnumerable<MeetingReadDTO>>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<MeetingReadDTO>>>
        GetByParticipant(int participantId)
    {
        try
        {
            var meetings =
                await _service.GetByParticipantAsync(participantId);

            return Ok(meetings);
        }
        catch (ValidationException exception)
        {
            return BadRequest(new
            {
                message = "Не вдалося отримати зустрічі учасника.",
                errors = exception.Errors
            });
        }
    }
}