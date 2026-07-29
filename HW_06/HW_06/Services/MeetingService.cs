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

    /// <summary>
    /// Ініціалізує новий екземпляр сервісу зустрічей.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="mapper">
    /// Сервіс AutoMapper.
    /// </param>
    /// <param name="createValidator">
    /// Валідатор створення зустрічі.
    /// </param>
    /// <param name="updateValidator">
    /// Валідатор повного оновлення зустрічі.
    /// </param>
    /// <param name="partialValidator">
    /// Валідатор часткового оновлення зустрічі.
    /// </param>
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

    /// <summary>
    /// Отримує сторінку зі списком зустрічей.
    /// </summary>
    /// <param name="filter">
    /// Параметри фільтрації.
    /// </param>
    /// <param name="parameters">
    /// Параметри пошуку, сортування та пагінації.
    /// </param>
    /// <returns>
    /// Сторінка зі списком зустрічей.
    /// </returns>
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

    /// <summary>
    /// Отримує детальну інформацію про зустріч.
    /// </summary>
    /// <param name="id">
    /// Ідентифікатор зустрічі.
    /// </param>
    /// <returns>
    /// Детальна інформація про зустріч або
    /// <see langword="null"/>, якщо зустріч не знайдено.
    /// </returns>
    public async Task<MeetingDetailDTO?> GetByIdAsync(int id)
    {
        var meeting = await _context.Meetings
            .AsNoTracking()
            .Include(meeting => meeting.Room)
            .Include(meeting => meeting.MeetingParticipants)
            .ThenInclude(meetingParticipant =>
                meetingParticipant.Participant)
            .FirstOrDefaultAsync(meeting =>
                meeting.MeetingId == id);

        if (meeting is null)
        {
            return null;
        }

        return _mapper.Map<MeetingDetailDTO>(meeting);
    }

    /// <summary>
    /// Створює нову зустріч.
    /// </summary>
    /// <param name="dto">
    /// Дані нової зустрічі.
    /// </param>
    /// <returns>
    /// Створена зустріч.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Виникає, якщо дані не пройшли перевірку,
    /// кімнату або учасників не знайдено.
    /// </exception>
    public async Task<MeetingReadDTO> CreateAsync(
        MeetingCreateDTO dto)
    {
        var validationResult = _createValidator.Validate(dto);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(
                validationResult.Errors);
        }

        Room? room = null;

        if (dto.RoomNumber.HasValue)
        {
            room = await _context.Rooms
                .AsNoTracking()
                .FirstOrDefaultAsync(item =>
                    item.NumberRoom ==
                    dto.RoomNumber.Value);

            if (room is null)
            {
                throw new ValidationException(
                    nameof(dto.RoomNumber),
                    "Кімнату із зазначеним номером не знайдено.");
            }
        }

        var participantIds = dto.ParticipantIds
            .Distinct()
            .ToList();

        var existingParticipantIds =
            await _context.Participants
                .Where(participant =>
                    participantIds.Contains(
                        participant.ParticipantId))
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

        meeting.RoomId = room?.RoomId;

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

        return _mapper.Map<MeetingReadDTO>(
            createdMeeting);
    }

    /// <summary>
    /// Повністю оновлює існуючу зустріч.
    /// </summary>
    /// <param name="id">
    /// Ідентифікатор зустрічі з адреси запиту.
    /// </param>
    /// <param name="dto">
    /// Нові дані зустрічі.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, якщо зустріч оновлено;
    /// інакше <see langword="false"/>.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Виникає, якщо дані не пройшли перевірку,
    /// кімнату або учасників не знайдено.
    /// </exception>
    public async Task<bool> UpdateAsync(
        int id,
        MeetingUpdateDTO dto)
    {
        if (id != dto.MeetingId)
        {
            throw new ValidationException(
                nameof(dto.MeetingId),
                "Ідентифікатор зустрічі в адресі не збігається " +
                "з ідентифікатором у тілі запиту.");
        }

        var validationResult =
            _updateValidator.Validate(dto);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(
                validationResult.Errors);
        }

        var meeting = await _context.Meetings
            .Include(item => item.MeetingParticipants)
            .FirstOrDefaultAsync(item =>
                item.MeetingId == id);

        if (meeting is null)
        {
            return false;
        }

        Room? room = null;

        if (dto.RoomNumber.HasValue)
        {
            room = await _context.Rooms
                .AsNoTracking()
                .FirstOrDefaultAsync(item =>
                    item.NumberRoom ==
                    dto.RoomNumber.Value);

            if (room is null)
            {
                throw new ValidationException(
                    nameof(dto.RoomNumber),
                    "Кімнату із зазначеним номером не знайдено.");
            }
        }

        var participantIds = dto.ParticipantIds
            .Distinct()
            .ToList();

        var existingParticipantIds =
            await _context.Participants
                .Where(participant =>
                    participantIds.Contains(
                        participant.ParticipantId))
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

        meeting.Title = dto.Title;
        meeting.Description = dto.Description;
        meeting.DateTime = dto.DateTime;
        meeting.RoomId = room?.RoomId;

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

    /// <summary>
    /// Частково оновлює існуючу зустріч.
    /// </summary>
    /// <param name="id">
    /// Ідентифікатор зустрічі.
    /// </param>
    /// <param name="dto">
    /// Поля зустрічі, які необхідно оновити.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, якщо зустріч оновлено;
    /// інакше <see langword="false"/>.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Виникає, якщо дані не пройшли перевірку
    /// або кімнату не знайдено.
    /// </exception>
    public async Task<bool> PartialUpdateAsync(
        int id,
        MeetingPartialUpdateDTO dto)
    {
        var validationResult =
            _partialValidator.Validate(dto);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(
                validationResult.Errors);
        }

        var meeting = await _context.Meetings
            .FirstOrDefaultAsync(item =>
                item.MeetingId == id);

        if (meeting is null)
        {
            return false;
        }

        Room? room = null;

        if (dto.RoomNumber.HasValue)
        {
            room = await _context.Rooms
                .AsNoTracking()
                .FirstOrDefaultAsync(item =>
                    item.NumberRoom ==
                    dto.RoomNumber.Value);

            if (room is null)
            {
                throw new ValidationException(
                    nameof(dto.RoomNumber),
                    "Кімнату із зазначеним номером не знайдено.");
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
            meeting.DateTime =
                dto.DateTime.Value;
        }

        if (room is not null)
        {
            meeting.RoomId = room.RoomId;
        }

        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Видаляє зустріч за ідентифікатором.
    /// </summary>
    /// <param name="id">
    /// Ідентифікатор зустрічі.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, якщо зустріч видалено;
    /// інакше <see langword="false"/>.
    /// </returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var meeting = await _context.Meetings
            .FirstOrDefaultAsync(item =>
                item.MeetingId == id);

        if (meeting is null)
        {
            return false;
        }

        _context.Meetings.Remove(meeting);

        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Видаляє декілька зустрічей.
    /// </summary>
    /// <param name="ids">
    /// Список ідентифікаторів зустрічей.
    /// </param>
    /// <returns>
    /// Кількість видалених зустрічей.
    /// </returns>
    public async Task<int> DeleteManyAsync(
        List<int> ids)
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
                validIds.Contains(
                    meeting.MeetingId))
            .ToListAsync();

        if (meetings.Count == 0)
        {
            return 0;
        }

        _context.Meetings.RemoveRange(meetings);

        await _context.SaveChangesAsync();

        return meetings.Count;
    }

    /// <summary>
    /// Отримує всі зустрічі зазначеного учасника.
    /// </summary>
    /// <param name="participantId">
    /// Ідентифікатор учасника.
    /// </param>
    /// <returns>
    /// Список зустрічей учасника.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Виникає, якщо ідентифікатор некоректний
    /// або учасника не знайдено.
    /// </exception>
    public async Task<List<MeetingReadDTO>>
        GetByParticipantAsync(int participantId)
    {
        if (participantId <= 0)
        {
            throw new ValidationException(
                nameof(participantId),
                "Ідентифікатор учасника повинен бути більшим за нуль.");
        }

        var participantExists =
            await _context.Participants
                .AnyAsync(participant =>
                    participant.ParticipantId ==
                    participantId);

        if (!participantExists)
        {
            throw new ValidationException(
                nameof(participantId),
                "Учасника із зазначеним ідентифікатором не знайдено.");
        }

        var meetings = await _context.Meetings
            .AsNoTracking()
            .Include(meeting => meeting.Room)
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

        return _mapper.Map<List<MeetingReadDTO>>(
            meetings);
    }
}