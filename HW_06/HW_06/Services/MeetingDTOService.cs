using AutoMapper;
using HW_06.DTOs.Meeting;
using Microsoft.EntityFrameworkCore;
using HW_06.Models;
using AutoMapper.QueryableExtensions;

namespace HW_06.Services;

public class MeetingDTOService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public MeetingDTOService(
        DataContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<MeetingreadDTO>> GetMeetings(MeetingFilter filter)
    {
        var query = _context.Meetings
            .AsNoTracking()
            .Include(m => m.Room)
            .Include(m => m.MeetingParticipants)
            .AsQueryable();
    
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(x => x.Title.Contains(filter.Search));
        }

        if (filter.StartTime.HasValue)
        {
            query = query.Where(x => x.DateTime >= filter.StartTime.Value);
        }

        if (filter.EndTime.HasValue)
        {
            query = query.Where(x => x.DateTime <= filter.EndTime.Value);
        }
        
        switch (filter.SortBy?.ToLower())
        {
            case "title":
                query = query.OrderBy(x => x.Title);
                break;

            case "date":
                query = query.OrderBy(x => x.DateTime);
                break;
        }
        
        var meetings = await query.ToListAsync();
        foreach (var meeting in meetings)
        {
            Console.WriteLine(
                $"{meeting.Title} | RoomId={meeting.RoomId} | Room={meeting.Room?.NumberRoom} | Participants={meeting.MeetingParticipants.Count}");
        }

        return _mapper.Map<List<MeetingreadDTO>>(meetings);
    }
    
    public async Task<MeetingditeylDTO?> GetById(int id)
    {
        var meeting = await _context.Meetings
            .AsNoTracking()
            .Include(m => m.Room)
            .Include(m => m.MeetingParticipants)
            .ThenInclude(mp => mp.Participant)
            .FirstOrDefaultAsync(m => m.MeetingId == id);

        if (meeting == null)
            return null;

        return _mapper.Map<MeetingditeylDTO>(meeting);
    }

    public async Task Create(MeetingcreateDTO dto)
    {
        var meeting = _mapper.Map<Meeting>(dto);

        foreach (var participantId in dto.ParticipantIds)
        {
            meeting.MeetingParticipants.Add(new MeetingParticipant
            {
                ParticipantId = participantId
            });
        }

        _context.Meetings.Add(meeting);

        await _context.SaveChangesAsync();
    }
    
    public async Task<bool> Update(int id, MeetingupdateDTO dto)
    {
        if (id != dto.MeetingId)
            return false;

        var meeting = await _context.Meetings
            .Include(m => m.MeetingParticipants)
            .FirstOrDefaultAsync(m => m.MeetingId == id);

        if (meeting == null)
            return false;

        // Оновлення основних полів
        meeting.Title = dto.Title;
        meeting.Description = dto.Description;
        meeting.DateTime = dto.DateTime;
        meeting.RoomId = dto.RoomId;

        // Видалення старих зв'язків з учасниками
        meeting.MeetingParticipants.Clear();

        // Додавання нових учасників
        foreach (var participantId in dto.ParticipantIds)
        {
            meeting.MeetingParticipants.Add(new MeetingParticipant
            {
                ParticipantId = participantId
            });
        }

        await _context.SaveChangesAsync();

        return true;
    }
    
    public async Task<bool> PartialUpdate(int id, MeetingpartialUpdateDTO dto)
    {
        var meeting = await _context.Meetings
            .FirstOrDefaultAsync(m => m.MeetingId == id);

        if (meeting == null)
            return false;

        // Оновлення лише тих полів, які були передані

        if (dto.Title != null)
            meeting.Title = dto.Title;

        if (dto.Description != null)
            meeting.Description = dto.Description;

        if (dto.DateTime.HasValue)
            meeting.DateTime = dto.DateTime.Value;

        if (dto.RoomId.HasValue)
            meeting.RoomId = dto.RoomId;

        await _context.SaveChangesAsync();

        return true;
    }
    
    public async Task<bool> Delete(int id)
    {
        var meeting = await _context.Meetings
            .FirstOrDefaultAsync(m => m.MeetingId == id);

        if (meeting == null)
            return false;

        _context.Meetings.Remove(meeting);

        await _context.SaveChangesAsync();

        return true;
    }
    
    // Отримання всіх зустрічей конкретного учасника
    public async Task<List<MeetingreadDTO>> GetByParticipant(int participantId)
    {
        return await _context.Meetings
            .AsNoTracking()
            .Include(m => m.Room)
            .Include(m => m.MeetingParticipants)
            .Where(m => m.MeetingParticipants
                .Any(mp => mp.ParticipantId == participantId))
            .ProjectTo<MeetingreadDTO>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }
}