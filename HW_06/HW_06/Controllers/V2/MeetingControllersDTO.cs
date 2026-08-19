using Asp.Versioning;
using HW_06.DTOs.MeetingDTO;
using HW_06.Filters;
using HW_06.Helpers.Pagination;
using HW_06.Helpers.QueryParameters;
using HW_06.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
    private readonly IMeetingService _service;

    /// <summary>
    /// Ініціалізує контролер зустрічей.
    /// </summary>
    /// <param name="service">
    /// Сервіс для роботи із зустрічами.
    /// </param>
    public MeetingControllersDTO(
        IMeetingService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        _service = service;
    }

    /// <summary>
    /// Отримує список зустрічей
    /// із підтримкою пошуку, сортування,
    /// фільтрації та пагінації.
    /// </summary>
    /// <param name="filter">
    /// Параметри фільтрації зустрічей.
    /// </param>
    /// <param name="parameters">
    /// Параметри пошуку, сортування та пагінації.
    /// </param>
    /// <returns>
    /// Сторінка зі списком зустрічей.
    /// </returns>
    /// <response code="200">
    /// Список зустрічей успішно отримано.
    /// </response>
    [HttpGet]
    [ProducesResponseType<PagedResult<MeetingReadDTO>>(
        StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<MeetingReadDTO>>>
        GetMeetings(
            [FromQuery] MeetingFilter filter,
            [FromQuery] MeetingQueryParameters parameters)
    {
        var meetings =
            await _service.GetAllAsync(
                filter,
                parameters);

        return Ok(meetings);
    }

    /// <summary>
    /// Отримує детальну інформацію
    /// про зустріч за ідентифікатором.
    /// </summary>
    /// <param name="id">
    /// Ідентифікатор зустрічі.
    /// </param>
    /// <returns>
    /// Детальна інформація про зустріч.
    /// </returns>
    /// <response code="200">
    /// Зустріч успішно знайдено.
    /// </response>
    /// <response code="404">
    /// Зустріч із зазначеним ідентифікатором не знайдено.
    /// </response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<MeetingDetailDTO>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MeetingDetailDTO>>
        GetById(int id)
    {
        var meeting =
            await _service.GetByIdAsync(id);

        if (meeting is null)
        {
            return NotFound(new
            {
                message =
                    $"Зустріч з ідентифікатором {id} не знайдено."
            });
        }

        return Ok(meeting);
    }

    /// <summary>
    /// Створює нову зустріч.
    /// </summary>
    /// <param name="dto">
    /// Дані нової зустрічі.
    /// </param>
    /// <returns>
    /// Створена зустріч.
    /// </returns>
    /// <response code="201">
    /// Зустріч успішно створено.
    /// </response>
    /// <response code="400">
    /// Передані дані не пройшли перевірку
    /// або пов'язані сутності не знайдено.
    /// </response>
    [HttpPost]
    [ServiceFilter(typeof(
        ValidationFilter<MeetingCreateDTO>))]
    [ProducesResponseType<MeetingReadDTO>(
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MeetingReadDTO>>
        Create(
            [FromBody] MeetingCreateDTO dto)
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

    /// <summary>
    /// Повністю оновлює існуючу зустріч.
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
    /// Передані дані не пройшли перевірку
    /// або пов'язані сутності не знайдено.
    /// </response>
    /// <response code="404">
    /// Зустріч із зазначеним ідентифікатором не знайдено.
    /// </response>
    [HttpPut("{id:int}")]
    [ServiceFilter(typeof(
        ValidationFilter<MeetingUpdateDTO>))]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] MeetingUpdateDTO dto)
    {
        var updated =
            await _service.UpdateAsync(
                id,
                dto);

        if (!updated)
        {
            return NotFound(new
            {
                message =
                    $"Зустріч з ідентифікатором {id} не знайдено."
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Частково оновлює існуючу зустріч.
    /// Змінюються лише передані поля.
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
    /// Передані дані не пройшли перевірку
    /// або пов'язані сутності не знайдено.
    /// </response>
    /// <response code="404">
    /// Зустріч із зазначеним ідентифікатором не знайдено.
    /// </response>
    [HttpPatch("{id:int}")]
    [ServiceFilter(typeof(
        ValidationFilter<MeetingPartialUpdateDTO>))]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PartialUpdate(
        int id,
        [FromBody] MeetingPartialUpdateDTO dto)
    {
        var updated =
            await _service.PartialUpdateAsync(
                id,
                dto);

        if (!updated)
        {
            return NotFound(new
            {
                message =
                    $"Зустріч з ідентифікатором {id} не знайдено."
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Видаляє зустріч за ідентифікатором.
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
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id)
    {
        var deleted =
            await _service.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message =
                    $"Зустріч з ідентифікатором {id} не знайдено."
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Видаляє декілька зустрічей
    /// за списком ідентифікаторів.
    /// </summary>
    /// <param name="ids">
    /// Список ідентифікаторів зустрічей.
    /// </param>
    /// <returns>
    /// Кількість видалених зустрічей.
    /// </returns>
    /// <response code="200">
    /// Повертає кількість видалених зустрічей.
    /// </response>
    /// <response code="400">
    /// Список ідентифікаторів не передано
    /// або він порожній.
    /// </response>
    [HttpDelete("delete-many")]
    [ProducesResponseType<int>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>>
        DeleteMany(
            [FromBody] List<int>? ids)
    {
        if (ids is null ||
            ids.Count == 0)
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
    /// Отримує всі зустрічі,
    /// у яких бере участь зазначений учасник.
    /// </summary>
    /// <param name="participantId">
    /// Ідентифікатор учасника.
    /// </param>
    /// <returns>
    /// Список зустрічей учасника.
    /// </returns>
    /// <response code="200">
    /// Список зустрічей успішно отримано.
    /// </response>
    /// <response code="400">
    /// Передано некоректний ідентифікатор учасника.
    /// </response>
    /// <response code="404">
    /// Учасника із зазначеним ідентифікатором не знайдено.
    /// </response>
    [HttpGet(
        "by-participant/{participantId:int}")]
    [ProducesResponseType<IEnumerable<MeetingReadDTO>>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<MeetingReadDTO>>>
        GetByParticipant(
            int participantId)
    {
        var meetings =
            await _service.GetByParticipantAsync(
                participantId);

        return Ok(meetings);
    }
}