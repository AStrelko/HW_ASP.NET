using HW_06.DTOs.Participant;
using HW_06.Services;
using Microsoft.AspNetCore.Mvc;


namespace HW_06.Controllers;

[ApiController]
[Route("api/participantDTO")]
public class ParticipantControllersDTO: ControllerBase
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
    [HttpPatch("{id}")]
    public async Task<IActionResult> PartialUpdate(
        int id,
        ParticipantPartialUpdateDTO dto)
    {
        await _service.PartialUpdate(id, dto);

        return Ok();
    }
    
    /// <summary>
    /// Видалення учасника.
    /// </summary>
    /// <param name="id">Ідентифікатор учасника.</param>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.Delete(id);

        return Ok();
    }
}