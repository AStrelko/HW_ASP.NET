using HW_06.DTOs.Participant;
using HW_06.Services;
using Microsoft.AspNetCore.Mvc;

namespace HW_06.Controllers;

/// <summary>
/// Контролер для керування учасниками зустрічей.
/// Надає CRUD-операції та отримання списку зустрічей учасника.
/// </summary>
[ApiController]
[Route("api/participantDTO")]
public class ParticipantControllersDTO : ControllerBase
{
    private readonly ParticipantDTOService _service;

    public ParticipantControllersDTO(
        ParticipantDTOService service)
    {
        _service = service;
    }

    /// <summary>
    /// Отримання списку всіх учасників.
    /// </summary>
    /// <returns>Список учасників.</returns>
    [HttpGet]
    public async Task<IActionResult> GetParticipants()
    {
        var participants = await _service.GetParticipants();

        return Ok(participants);
    }

    /// <summary>
    /// Отримання інформації про учасника за його ідентифікатором.
    /// </summary>
    /// <param name="id">Ідентифікатор учасника.</param>
    /// <returns>Інформація про учасника.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var participant = await _service.GetById(id);

        if (participant == null)
            return NotFound();

        return Ok(participant);
    }

    /// <summary>
    /// Створення нового учасника.
    /// </summary>
    /// <param name="dto">Дані нового учасника.</param>
    /// <returns>Результат створення учасника.</returns>
    [HttpPost]
    public async Task<IActionResult> Create(ParticipantCreateDTO dto)
    {
        await _service.Create(dto);

        return Ok();
    }

    /// <summary>
    /// Повне оновлення інформації про учасника.
    /// </summary>
    /// <param name="id">Ідентифікатор учасника.</param>
    /// <param name="dto">Нові дані учасника.</param>
    /// <returns>Результат оновлення учасника.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ParticipantUpdateDTO dto)
    {
        if (id != dto.ParticipantId)
            return BadRequest();

        await _service.Update(dto);

        return Ok();
    }

    /// <summary>
    /// Часткове оновлення інформації про учасника.
    /// </summary>
    /// <param name="id">Ідентифікатор учасника.</param>
    /// <param name="dto">Поля для оновлення.</param>
    /// <returns>Результат часткового оновлення.</returns>
    [HttpPatch("{id}")]
    public async Task<IActionResult> PartialUpdate(
        int id,
        ParticipantPartialUpdateDTO dto)
    {
        await _service.PartialUpdate(id, dto);

        return Ok();
    }

    /// <summary>
    /// Отримання всіх зустрічей, у яких бере участь вказаний учасник.
    /// </summary>
    /// <param name="participantId">Ідентифікатор учасника.</param>
    /// <returns>Список зустрічей.</returns>
    [HttpGet("by-participant/{participantId}")]
    public async Task<IActionResult> GetByParticipant(int participantId)
    {
        var meetings = await _service.GetByParticipant(participantId);

        return Ok(meetings);
    }

    /// <summary>
    /// Видалення учасника.
    /// </summary>
    /// <param name="id">Ідентифікатор учасника.</param>
    /// <returns>Результат видалення учасника.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.Delete(id);

        return Ok();
    }
}