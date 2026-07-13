using HW_06.Models;
using HW_06.Services;
using Microsoft.AspNetCore.Mvc;

namespace HW_06.Controllers;

[ApiController]
[Route("api/meetings")]
public class MeetingsController : ControllerBase
{
    private readonly MeetingServices _service;

    public MeetingsController(MeetingServices service)
    {
        _service = service;
    }
    // Отримання списку зустрічей з підтримкою
    // пошуку, сортування, фільтрації та пагінації.
    [HttpGet]
    public async Task<IActionResult> GetMeetings(
        [FromQuery] MeetingFilter filter)
    {
        return Ok(await _service.GetMeetings(filter));
    }

    // Отримання зустрічі за ідентифікатором.
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var meeting = await _service.GetById(id);

        if (meeting == null)
            return NotFound();

        return Ok(meeting);
    }

    // Створення нової зустрічі.
    // Якщо передані параметри Days та Count,
    // створюється серія зустрічей через задану кількість днів.
    [HttpPost]
    public async Task<IActionResult> Create(CreateMeetingRequest request)
    {
        if (request.Days.HasValue &&
            request.Count.HasValue)
        {
            for (int i = 0; i < request.Count.Value; i++)
            {
                var meeting = new Meeting
                {
                    Title = request.Title,
                    Description = request.Description,
                    DateTime = request.DateTime
                        .AddDays(i * request.Days.Value)
                };

                await _service.Add(meeting);
            }

            return Ok();
        }

        var singleMeeting = new Meeting
        {
            Title = request.Title,
            Description = request.Description,
            DateTime = request.DateTime
        };

        await _service.Add(singleMeeting);

        return Ok();
    }

    // Оновлення даних існуючої зустрічі.
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        Meeting meeting)
    {
        if (id != meeting.MeetingId)
            return BadRequest();

        await _service.Update(meeting);

        return Ok();
    }
    
    // Видалення зустрічі за ідентифікатором.
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.Delete(id);

        return Ok();
    }
    
    
    // Видалення декількох зустрічей за списком id
    [HttpDelete("delete-many")]
    public async Task<IActionResult> DeleteMany([FromBody] List<int> ids)
    {
        await  _service.DeleteMany(ids);
        return Ok();
    }
}