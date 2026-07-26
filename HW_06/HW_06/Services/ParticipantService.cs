using AutoMapper;
using HW_06.DTOs.MeetingDTO;
using HW_06.DTOs.ParticipantDTO;
using HW_06.Helpers.Pagination;
using HW_06.Helpers.QueryParameters;
using HW_06.Services.Interfaces;
using HW_06.Validators;
using HW_06.Helpers.Queryable;
using HW_06.Models;
using Microsoft.EntityFrameworkCore;
using HW_06.Validators.Exceptions;

namespace HW_06.Services;

public class ParticipantService : IParticipantService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    private readonly IValidator<ParticipantCreateDTO>
        _createValidator;

    private readonly IValidator<ParticipantUpdateDTO>
        _updateValidator;

    private readonly IValidator<ParticipantPartialUpdateDTO>
        _partialValidator;

    public ParticipantService(
        DataContext context,
        IMapper mapper,
        IValidator<ParticipantCreateDTO> createValidator,
        IValidator<ParticipantUpdateDTO> updateValidator,
        IValidator<ParticipantPartialUpdateDTO> partialValidator)
    {
        _context = context;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _partialValidator = partialValidator;
    }

    public async Task<PagedResult<ParticipantReadDTO>> GetAllAsync(
        ParticipantQueryParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        IQueryable<Participant> query = _context.Participants
            .AsNoTracking();

        query = query
            .ApplySearch(parameters.SearchLastName)
            .ApplySorting(parameters);

        return await query
            .ToPagedResultAsync<Participant, ParticipantReadDTO>(
                parameters.Page,
                parameters.PageSize,
                _mapper);
    }

    public async Task<ParticipantDetailDTO?> GetByIdAsync(int id)
    {
        var participant = await _context.Participants
            .AsNoTracking()
            .Include(participant =>
                participant.MeetingParticipants)
            .ThenInclude(meetingParticipant =>
                meetingParticipant.Meeting)
            .ThenInclude(meeting =>
                meeting.Room)
            .FirstOrDefaultAsync(participant =>
                participant.ParticipantId == id);

        if (participant is null)
        {
            return null;
        }

        var result = _mapper.Map<ParticipantDetailDTO>(
            participant);

        var meetingIds = participant.MeetingParticipants
            .Select(meetingParticipant =>
                meetingParticipant.MeetingId)
            .ToList();

        var participantCounts = await _context.MeetingParticipants
            .AsNoTracking()
            .Where(meetingParticipant =>
                meetingIds.Contains(
                    meetingParticipant.MeetingId))
            .GroupBy(meetingParticipant =>
                meetingParticipant.MeetingId)
            .Select(group => new
            {
                MeetingId = group.Key,
                ParticipantsCount = group.Count()
            })
            .ToDictionaryAsync(
                item => item.MeetingId,
                item => item.ParticipantsCount);

        foreach (var meeting in result.Meetings)
        {
            meeting.ParticipantsCount =
                participantCounts.GetValueOrDefault(
                    meeting.MeetingId);
        }

        return result;
    }
    
    public async Task<ParticipantReadDTO> CreateAsync(
        ParticipantCreateDTO dto)
    {
        var validationResult =
            _createValidator.Validate(dto);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(
                validationResult.Errors);
        }

        var normalizedEmail = dto.Email
            .Trim()
            .ToLowerInvariant();

        var emailExists = await _context.Participants
            .AnyAsync(participant =>
                participant.Email.ToLower() ==
                normalizedEmail);

        if (emailExists)
        {
            throw new ValidationException(
                nameof(dto.Email),
                "Учасник із такою електронною поштою вже існує.");
        }

        var meetingIds = dto.MeetingIds?
            .Distinct()
            .ToList() ?? new List<int>();

        await ValidateMeetingIdsAsync(
            meetingIds,
            nameof(dto.MeetingIds));

        var participant =
            _mapper.Map<Participant>(dto);

        participant.FirstName =
            participant.FirstName.Trim();

        participant.LastName =
            participant.LastName.Trim();

        participant.Email =
            normalizedEmail;

        participant.Role =
            participant.Role?.Trim();

        participant.MeetingParticipants = meetingIds
            .Select(meetingId =>
                new MeetingParticipant
                {
                    MeetingId = meetingId
                })
            .ToList();

        _context.Participants.Add(participant);

        await _context.SaveChangesAsync();

        return _mapper.Map<ParticipantReadDTO>(
            participant);
    }

    public async Task<bool> UpdateAsync(
        int id,
        ParticipantUpdateDTO dto)
    {
        var validationResult =
            _updateValidator.Validate(dto);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(
                validationResult.Errors);
        }

        var participant = await _context.Participants
            .Include(participant =>
                participant.MeetingParticipants)
            .FirstOrDefaultAsync(participant =>
                participant.ParticipantId == id);

        if (participant is null)
        {
            return false;
        }

        var normalizedEmail = dto.Email
            .Trim()
            .ToLowerInvariant();

        var emailExists = await _context.Participants
            .AnyAsync(otherParticipant =>
                otherParticipant.ParticipantId != id &&
                otherParticipant.Email.ToLower() ==
                normalizedEmail);

        if (emailExists)
        {
            throw new ValidationException(
                nameof(dto.Email),
                "Учасник із такою електронною поштою вже існує.");
        }

        var meetingIds = dto.MeetingIds
            .Distinct()
            .ToList();

        await ValidateMeetingIdsAsync(
            meetingIds,
            nameof(dto.MeetingIds));

        participant.FirstName =
            dto.FirstName.Trim();

        participant.LastName =
            dto.LastName.Trim();

        participant.Email =
            normalizedEmail;

        participant.Role =
            dto.Role?.Trim();

        participant.MeetingParticipants.Clear();

        foreach (var meetingId in meetingIds)
        {
            participant.MeetingParticipants.Add(
                new MeetingParticipant
                {
                    ParticipantId = participant.ParticipantId,
                    MeetingId = meetingId
                });
        }

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> PartialUpdateAsync(
    int id,
    ParticipantPartialUpdateDTO dto)
{
    var validationResult =
        _partialValidator.Validate(dto);

    if (!validationResult.IsValid)
    {
        throw new ValidationException(
            validationResult.Errors);
    }

    var participant = await _context.Participants
        .Include(participant =>
            participant.MeetingParticipants)
        .FirstOrDefaultAsync(participant =>
            participant.ParticipantId == id);

    if (participant is null)
    {
        return false;
    }

    if (dto.FirstName is not null)
    {
        participant.FirstName =
            dto.FirstName.Trim();
    }

    if (dto.LastName is not null)
    {
        participant.LastName =
            dto.LastName.Trim();
    }

    if (dto.Email is not null)
    {
        var normalizedEmail = dto.Email
            .Trim()
            .ToLowerInvariant();

        var emailExists = await _context.Participants
            .AnyAsync(otherParticipant =>
                otherParticipant.ParticipantId != id &&
                otherParticipant.Email.ToLower() ==
                normalizedEmail);

        if (emailExists)
        {
            throw new ValidationException(
                nameof(dto.Email),
                "Учасник із такою електронною поштою вже існує.");
        }

        participant.Email =
            normalizedEmail;
    }

    if (dto.Role is not null)
    {
        participant.Role =
            dto.Role.Trim();
    }

    if (dto.MeetingIds is not null)
    {
        var meetingIds = dto.MeetingIds
            .Distinct()
            .ToList();

        await ValidateMeetingIdsAsync(
            meetingIds,
            nameof(dto.MeetingIds));

        participant.MeetingParticipants.Clear();

        foreach (var meetingId in meetingIds)
        {
            participant.MeetingParticipants.Add(
                new MeetingParticipant
                {
                    ParticipantId =
                        participant.ParticipantId,

                    MeetingId = meetingId
                });
        }
    }

    await _context.SaveChangesAsync();

    return true;
}

    public async Task<bool> DeleteAsync(int id)
    {
        var participant = await _context.Participants
            .Include(participant =>
                participant.MeetingParticipants)
            .FirstOrDefaultAsync(participant =>
                participant.ParticipantId == id);

        if (participant is null)
        {
            return false;
        }

        _context.MeetingParticipants.RemoveRange(
            participant.MeetingParticipants);

        _context.Participants.Remove(participant);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> DeleteManyAsync(List<int> ids)
    {
        ArgumentNullException.ThrowIfNull(ids);

        var participantIds = ids
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (participantIds.Count == 0)
        {
            return 0;
        }

        var participants = await _context.Participants
            .Include(participant =>
                participant.MeetingParticipants)
            .Where(participant =>
                participantIds.Contains(
                    participant.ParticipantId))
            .ToListAsync();

        if (participants.Count == 0)
        {
            return 0;
        }

        var meetingParticipants = participants
            .SelectMany(participant =>
                participant.MeetingParticipants)
            .ToList();

        _context.MeetingParticipants.RemoveRange(
            meetingParticipants);

        _context.Participants.RemoveRange(
            participants);

        await _context.SaveChangesAsync();

        return participants.Count;
    }

 

    public async Task<List<MeetingReadDTO>> GetMeetingsAsync(
        int participantId)
    {
        var meetings = await _context.Meetings
            .AsNoTracking()
            .Include(meeting =>
                meeting.Room)
            .Include(meeting =>
                meeting.MeetingParticipants)
            .Where(meeting =>
                meeting.MeetingParticipants.Any(
                    meetingParticipant =>
                        meetingParticipant.ParticipantId ==
                        participantId))
            .OrderBy(meeting =>
                meeting.DateTime)
            .ToListAsync();

        var result =
            _mapper.Map<List<MeetingReadDTO>>(meetings);

        var participantCounts = meetings
            .ToDictionary(
                meeting => meeting.MeetingId,
                meeting => meeting.MeetingParticipants.Count);

        foreach (var meeting in result)
        {
            meeting.ParticipantsCount =
                participantCounts.GetValueOrDefault(
                    meeting.MeetingId);
        }

        return result;
    }
    
    private async Task ValidateMeetingIdsAsync(List<int> meetingIds, string propertyName)
    {
        if (meetingIds.Count == 0)
        {
            return;
        }

        var existingMeetingIds = await _context.Meetings
            .AsNoTracking()
            .Where(meeting => meetingIds.Contains(meeting.MeetingId))
            .Select(meeting => meeting.MeetingId)
            .ToListAsync();

        var missingMeetingIds = meetingIds
            .Except(existingMeetingIds)
            .ToList();

        if (missingMeetingIds.Count > 0)
        {
            throw new ValidationException(
                propertyName,
                $"Зустрічі не знайдено: " +
                $"{string.Join(", ", missingMeetingIds)}.");
        }
    }
}