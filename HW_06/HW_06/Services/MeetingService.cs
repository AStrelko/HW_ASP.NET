using AutoMapper;
using HW_06.DTOs.MeetingDTO;
using HW_06.Helpers.Pagination;
using HW_06.Helpers.Queryable;
using HW_06.Helpers.QueryParameters;
using HW_06.Models;
using HW_06.Services.Interfaces;
using HW_06.Validators;
using HW_06.Validators.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Services;

/// <summary>
/// Сервіс для роботи із зустрічами.
/// </summary>
public class MeetingService : IMeetingService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    private readonly IValidator<MeetingCreateDTO> _createValidator;
    private readonly IValidator<MeetingUpdateDTO> _updateValidator;
    private readonly IValidator<MeetingPartialUpdateDTO> _partialValidator;

    public MeetingService(
        DataContext context,
        IMapper mapper,
        IValidator<MeetingCreateDTO> createValidator,
        IValidator<MeetingUpdateDTO> updateValidator,
        IValidator<MeetingPartialUpdateDTO> partialValidator)
    {
        _context = context;
        _mapper = mapper;

        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _partialValidator = partialValidator;
    }

    public async Task<PagedResult<MeetingReadDTO>> GetAllAsync(
        MeetingFilter filter,
        MeetingQueryParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(parameters);

        IQueryable<Meeting> query = _context.Meetings
            .AsNoTracking()
            .Include(meeting => meeting.Room)
            .Include(meeting =>
                meeting.MeetingParticipants);

        query = query
            .ApplySearch(parameters.Search)
            .ApplyFilter(filter)
            .ApplySorting(parameters);

        return await query
            .ToPagedResultAsync<Meeting, MeetingReadDTO>(
                parameters.Page,
                parameters.PageSize,
                _mapper);
    }

    public async Task<MeetingDetailDTO?> GetByIdAsync(int id)
    {
        var meeting = await _context.Meetings
            .AsNoTracking()
            .Include(meeting => meeting.Room)
            .Include(meeting => meeting.MeetingParticipants)
            .ThenInclude(meetingParticipant => meetingParticipant.Participant)
            .FirstOrDefaultAsync(meeting => meeting.MeetingId == id);
        if (meeting == null)
        {
            return null;
        }
        return _mapper.Map<MeetingDetailDTO>(meeting);
    }

    public async Task<MeetingReadDTO> CreateAsync(
        MeetingCreateDTO dto)
    {
        var validationResult = _createValidator.Validate(dto);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        if (dto.RoomId.HasValue)
        {
            var roomExists = await _context.Rooms
                .AnyAsync(room =>
                    room.RoomId == dto.RoomId.Value);

            if (!roomExists)
            {
                throw new ValidationException(
                    nameof(dto.RoomId),
                    "Кімнату із зазначеним ідентифікатором не знайдено.");
            }
        }

        var participantIds = dto.ParticipantIds
            .Distinct()
            .ToList();

        var existingParticipantIds = await _context.Participants
            .Where(participant =>
                participantIds.Contains(participant.ParticipantId))
            .Select(participant =>
                participant.ParticipantId)
            .ToListAsync();

        var missingParticipantIds = participantIds
            .Except(existingParticipantIds)
            .ToList();

        if (missingParticipantIds.Count > 0)
        {
            throw new ValidationException(
                nameof(dto.ParticipantIds),
                $"Не знайдено учасників з ідентифікаторами: " +
                $"{string.Join(", ", missingParticipantIds)}.");
        }

        var meeting = _mapper.Map<Meeting>(dto);

        meeting.MeetingParticipants = participantIds
            .Select(participantId =>
                new MeetingParticipant
                {
                    ParticipantId = participantId
                })
            .ToList();

        _context.Meetings.Add(meeting);

        await _context.SaveChangesAsync();

        var createdMeeting = await _context.Meetings
            .AsNoTracking()
            .Include(item => item.Room)
            .Include(item => item.MeetingParticipants)
            .FirstAsync(item =>
                item.MeetingId == meeting.MeetingId);

        return _mapper.Map<MeetingReadDTO>(createdMeeting);
    }

    public async Task<bool> UpdateAsync(
    int id,
    MeetingUpdateDTO dto)
{
    if (id != dto.MeetingId)
    {
        throw new ValidationException(
            nameof(dto.MeetingId),
            "Ідентифікатор зустрічі в адресі не збігається з ідентифікатором у тілі запиту.");
    }

    var validationResult = _updateValidator.Validate(dto);

    if (!validationResult.IsValid)
    {
        throw new ValidationException(validationResult.Errors);
    }

    var meeting = await _context.Meetings
        .Include(item => item.MeetingParticipants)
        .FirstOrDefaultAsync(item => item.MeetingId == id);

    if (meeting is null)
    {
        return false;
    }

    if (dto.RoomId.HasValue)
    {
        var roomExists = await _context.Rooms
            .AnyAsync(room => room.RoomId == dto.RoomId.Value);

        if (!roomExists)
        {
            throw new ValidationException(
                nameof(dto.RoomId),
                "Кімнату із зазначеним ідентифікатором не знайдено.");
        }
    }

    var participantIds = dto.ParticipantIds
        .Distinct()
        .ToList();

    var existingParticipantIds = await _context.Participants
        .Where(participant =>
            participantIds.Contains(participant.ParticipantId))
        .Select(participant => participant.ParticipantId)
        .ToListAsync();

    var missingParticipantIds = participantIds
        .Except(existingParticipantIds)
        .ToList();

    if (missingParticipantIds.Count > 0)
    {
        throw new ValidationException(
            nameof(dto.ParticipantIds),
            $"Не знайдено учасників з ідентифікаторами: " +
            $"{string.Join(", ", missingParticipantIds)}.");
    }

    meeting.Title = dto.Title;
    meeting.Description = dto.Description;
    meeting.DateTime = dto.DateTime;
    meeting.RoomId = dto.RoomId;

    meeting.MeetingParticipants.Clear();

    foreach (var participantId in participantIds)
    {
        meeting.MeetingParticipants.Add(
            new MeetingParticipant
            {
                MeetingId = meeting.MeetingId,
                ParticipantId = participantId
            });
    }

    await _context.SaveChangesAsync();

    return true;
}

    public async Task<bool> PartialUpdateAsync(
        int id,
        MeetingPartialUpdateDTO dto)
    {
        var validationResult = _partialValidator.Validate(dto);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var meeting = await _context.Meetings
            .FirstOrDefaultAsync(item => item.MeetingId == id);

        if (meeting is null)
        {
            return false;
        }

        if (dto.RoomId.HasValue)
        {
            var roomExists = await _context.Rooms
                .AnyAsync(room => room.RoomId == dto.RoomId.Value);

            if (!roomExists)
            {
                throw new ValidationException(
                    nameof(dto.RoomId),
                    "Room was not found.");
            }
        }

        if (dto.Title is not null)
        {
            meeting.Title = dto.Title;
        }

        if (dto.Description is not null)
        {
            meeting.Description = dto.Description;
        }

        if (dto.DateTime.HasValue)
        {
            meeting.DateTime = dto.DateTime.Value;
        }

        if (dto.RoomId.HasValue)
        {
            meeting.RoomId = dto.RoomId.Value;
        }

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var meeting = await _context.Meetings
            .FirstOrDefaultAsync(item => item.MeetingId == id);

        if (meeting is null)
        {
            return false;
        }

        _context.Meetings.Remove(meeting);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> DeleteManyAsync(List<int> ids)
    {
        var validIds = ids
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (validIds.Count == 0)
        {
            return 0;
        }

        var meetings = await _context.Meetings
            .Where(meeting =>
                validIds.Contains(meeting.MeetingId))
            .ToListAsync();

        if (meetings.Count == 0)
        {
            return 0;
        }

        _context.Meetings.RemoveRange(meetings);

        await _context.SaveChangesAsync();

        return meetings.Count;
    }

    public async Task<List<MeetingReadDTO>> GetByParticipantAsync(
        int participantId)
    {
        if (participantId <= 0)
        {
            throw new ValidationException(
                nameof(participantId),
                "Ідентифікатор учасника повинен бути більшим за нуль.");
        }

        var participantExists = await _context.Participants
            .AnyAsync(participant =>
                participant.ParticipantId == participantId);

        if (!participantExists)
        {
            throw new ValidationException(
                nameof(participantId),
                "Учасника із зазначеним ідентифікатором не знайдено.");
        }

        var meetings = await _context.Meetings
            .AsNoTracking()
            .Include(meeting => meeting.Room)
            .Include(meeting => meeting.MeetingParticipants)
            .Where(meeting =>
                meeting.MeetingParticipants.Any(
                    meetingParticipant =>
                        meetingParticipant.ParticipantId ==
                        participantId))
            .OrderBy(meeting => meeting.DateTime)
            .ToListAsync();

        return _mapper.Map<List<MeetingReadDTO>>(meetings);
    }
}