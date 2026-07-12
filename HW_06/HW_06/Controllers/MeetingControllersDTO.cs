using AutoMapper;
using HW_06.DTOs.Meeting;
using HW_06.Models;
using HW_06.Services;
using Microsoft.AspNetCore.Mvc;

namespace HW_06.Controllers;

[ApiController]
[Route("api/meetingsDTO")]
public class MeetingControllersDTO : ControllerBase
{
    private readonly MeetingServices _service;
    private readonly IMapper _mapper;

    public MeetingControllersDTO(
        MeetingServices service,
        IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    // Отримання списку зустрічей
    [HttpGet]
    public async Task<IActionResult> GetMeetings([FromQuery] MeetingFilter filter)
    {
        var meetings = await _service.GetMeetings(filter);

        var result = _mapper.Map<List<MeetingreadDTO>>(meetings);

        return Ok(result);
    }

    // Отримання зустрічі за id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var meeting = await _service.GetById(id);

        if (meeting == null)
            return NotFound();

        var result = _mapper.Map<MeetingditeylDTO>(meeting);

        return Ok(result);
    }

    // Створення нової зустрічі
    [HttpPost]
    public async Task<IActionResult> Create(MeetingcreateDTO dto)
    {
        var meeting = _mapper.Map<Meeting>(dto);

        await _service.Add(meeting);

        return Ok();
    }

    // Оновлення зустрічі
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, MeetingupdateDTO dto)
    {
        var meeting = _mapper.Map<Meeting>(dto);

        if (id != meeting.Id)
            return BadRequest();

        await _service.Update(meeting);

        return Ok();
    }
    
    //Часткове оновлення зустрічи
    [HttpPatch("{id}")]
    public async Task<IActionResult> PartialUpdate(int id, MeetingpartialUpdateDTO dto)
    {
        var meeting = await _service.GetById(id);

        if (meeting == null)
            return NotFound();

        if (dto.Title != null)
            meeting.Title = dto.Title;

        if (dto.Description != null)
            meeting.Description = dto.Description;

        if (dto.DateTime.HasValue)
            meeting.DateTime = dto.DateTime.Value;

        await _service.Update(meeting);

        return Ok();
    }

    // Видалення зустрічі
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.Delete(id);

        return Ok();
    }
}