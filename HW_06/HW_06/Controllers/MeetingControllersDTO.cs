using HW_06.DTOs.Meeting;
using HW_06.Services;
using Microsoft.AspNetCore.Mvc;

namespace HW_06.Controllers;

/// <summary>
/// Контролер для керування зустрічами.
/// Надає CRUD-операції, пошук, сортування, фільтрацію,
/// пагінацію та отримання зустрічей за учасником.
/// </summary>
[ApiController]
[Route("api/meetingsDTO")]
public class MeetingControllersDTO : ControllerBase
{
    private readonly MeetingDTOService _service;

    public MeetingControllersDTO(MeetingDTOService service)
    {
        _service = service;
        
    }

    /// <summary>
    /// Отримання списку зустрічей.
    /// Підтримує пошук, сортування, фільтрацію та пагінацію.
    /// </summary>
    /// <param name="filter">Параметри пошуку, сортування та пагінації.</param>
    /// <returns>Список зустрічей у скороченому вигляді.</returns>
    [HttpGet]
    public async Task<IActionResult> GetMeetings([FromQuery] MeetingFilter filter)
    {
        var meetings = await _service.GetMeetings(filter);

        return Ok(meetings);
    }

    /// <summary>
    /// Отримання детальної інформації про зустріч за її ідентифікатором.
    /// </summary>
    /// <param name="id">Ідентифікатор зустрічі.</param>
    /// <returns>Повна інформація про зустріч.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var meeting = await _service.GetById(id);
        
        if (meeting == null)
            return NotFound();
        return Ok(meeting);
    }

    /// <summary>
    /// Створення нової зустрічі.
    /// До зустрічі можна одразу прив'язати кімнату та учасників.
    /// </summary>
    /// <param name="dto">Дані нової зустрічі.</param>
    /// <returns>Результат створення зустрічі.</returns>
    [HttpPost]
    public async Task<IActionResult> Create(MeetingcreateDTO dto)
    {
        await _service.Create(dto);
        return Ok();
    }
    
    /// <summary>
    /// Повне оновлення зустрічі.
    /// </summary>
    /// <param name="id">Ідентифікатор зустрічі.</param>
    /// <param name="dto">Нові дані зустрічі.</param>
    /// <returns>Результат оновлення.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, MeetingupdateDTO dto)
    {
        var result = await _service.Update(id, dto);

        if (!result)
            return NotFound();

        return Ok();
    }
    
    /// <summary>
    /// Часткове оновлення зустрічі.
    /// </summary>
    /// <param name="id">Ідентифікатор зустрічі.</param>
    /// <param name="dto">Поля, які необхідно оновити.</param>
    /// <returns>Результат часткового оновлення.</returns>
    [HttpPatch("{id}")]
    public async Task<IActionResult> PartialUpdate(
        int id,
        MeetingpartialUpdateDTO dto)
    {
        var result = await _service.PartialUpdate(id, dto);

        if (!result)
            return NotFound();

        return Ok();
    }
    
    /// <summary>
    /// Видалення зустрічі за її ідентифікатором.
    /// </summary>
    /// <param name="id">Ідентифікатор зустрічі.</param>
    /// <returns>Результат видалення.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.Delete(id);

        if (!result)
            return NotFound();

        return NoContent();
    }
    
    /// <summary>
    /// Отримання всіх зустрічей конкретного учасника.
    /// </summary>
    /// <param name="participantId">
    /// Ідентифікатор учасника.
    /// </param>
    /// <returns>
    /// Список зустрічей, у яких бере участь зазначений учасник.
    /// </returns>
    [HttpGet("by-participant/{participantId}")]
    public async Task<IActionResult> GetByParticipant(int participantId)
    {
        var meetings = await _service.GetByParticipant(participantId);

        return Ok(meetings);
    }
}