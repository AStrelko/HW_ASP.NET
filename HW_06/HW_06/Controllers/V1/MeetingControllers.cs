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

    public MeetingControllers(IMeetingService service)
    {
        _service = service;
    }

    /// <summary>
    /// Отримання списку зустрічей.
    /// </summary>
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
    /// Отримання зустрічі за ідентифікатором.
    /// </summary>
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
    /// Створення нової зустрічі.
    /// </summary>
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
    /// Повне оновлення зустрічі.
    /// </summary>
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
    /// Видалення зустрічі.
    /// </summary>
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