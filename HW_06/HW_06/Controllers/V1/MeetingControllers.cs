using Asp.Versioning;
using HW_06.DTOs.MeetingDTO;
using HW_06.Helpers.QueryParameters;
using HW_06.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HW_06.Controllers.V1;

/// <summary>
/// Перша версія API для базового керування зустрічами.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/meetings")]
[Consumes("application/json")]
[Produces("application/json")]
public class MeetingControllers : ControllerBase
{
    private readonly IMeetingService _service;

    /// <summary>
    /// Ініціалізує новий екземпляр контролера зустрічей.
    /// </summary>
    /// <param name="service">
    /// Сервіс для роботи із зустрічами.
    /// </param>

    public MeetingControllers(IMeetingService service)
    {
        _service = service;
    }

    /// <summary>
    /// Отримує список усіх зустрічей.
    /// </summary>
    /// <returns>
    /// Колекцію зустрічей у скороченому форматі.
    /// </returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MeetingReadDTO>>>
        GetMeetings()
    {
        var filter = new MeetingFilter();

        var parameters = new MeetingQueryParameters
        {
            Page = 1,
            PageSize = 100
        };

        var result =
            await _service.GetAllAsync(filter, parameters);

        return Ok(result.Items);
    }

    /// <summary>
    /// Отримує інформацію про зустріч за її ідентифікатором.
    /// </summary>
    /// <param name="id">
    /// Ідентифікатор зустрічі.
    /// </param>
    /// <returns>
    /// Детальну інформацію про зустріч.
    /// </returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType<MeetingDetailDTO>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MeetingDetailDTO>>
        GetById(int id)
    {
        var meeting =
            await _service.GetByIdAsync(id);

        if (meeting is null)
            return NotFound();

        return Ok(meeting);
    }

    /// <summary>
    /// Створює нову зустріч.
    /// </summary>
    /// <param name="dto">
    /// Дані для створення зустрічі.
    /// </param>
    /// <returns>
    /// Створену зустріч.
    /// </returns>
    [HttpPost]
    [ProducesResponseType<MeetingReadDTO>(
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MeetingReadDTO>> Create(
        [FromBody] MeetingCreateDTO dto)
    {
        var createdMeeting =
            await _service.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                id = createdMeeting.MeetingId,
                version = "1.0"
            },
            createdMeeting);
    }

    /// <summary>
    /// Повністю оновлює інформацію про зустріч.
    /// </summary>
    /// <param name="id">
    /// Ідентифікатор зустрічі.
    /// </param>
    /// <param name="dto">
    /// Нові дані зустрічі.
    /// </param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] MeetingUpdateDTO dto)
    {
        var updated =
            await _service.UpdateAsync(id, dto);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Видаляє зустріч.
    /// </summary>
    /// <param name="id">
    /// Ідентифікатор зустрічі.
    /// </param>

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted =
            await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}