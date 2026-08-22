using AutoMapper;
using HW_06.DTOs.MeetingDTO;
using HW_06.Helpers.Pagination;
using HW_06.Helpers.Queryable;
using HW_06.Helpers.QueryParameters;
using HW_06.Models;
using HW_06.Services.Interfaces;
using HW_06.Validators.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Services;

/// <summary>
/// Сервіс для роботи із зустрічами.
/// Виконує бізнес-логіку та операції
/// з базою даних.
/// </summary>
public class MeetingService : IMeetingService
{
    /// <summary>
    /// Контекст бази даних застосунку.
    /// </summary>
    private readonly DataContext _context;

    /// <summary>
    /// Сервіс AutoMapper для перетворення
    /// моделей домену в DTO і навпаки.
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    /// Ініціалізує новий екземпляр сервісу зустрічей.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="mapper">
    /// Сервіс AutoMapper.
    /// </param>
    public MeetingService(
        DataContext context,
        IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(mapper);

        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Отримує сторінку зі списком зустрічей
    /// із підтримкою пошуку, фільтрації,
    /// сортування та пагінації.
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
    /// Отримує детальну інформацію
    /// про зустріч за ідентифікатором.
    /// </summary>
    /// <param name="id">
    /// Ідентифікатор зустрічі.
    /// </param>
    /// <returns>
    /// Детальна інформація про зустріч або
    /// <see langword="null"/>, якщо зустріч не знайдено.
    /// </returns>
    public async Task<MeetingDetailDTO?> GetByIdAsync(
        int id)
    {
        var meeting = await _context.Meetings
            .AsNoTracking()
            .Include(meeting =>
                meeting.Room)
            .Include(meeting =>
                meeting.MeetingParticipants)
            .ThenInclude(link =>
                link.Participant)
            .ThenInclude(participant =>
                participant.ApplicationUser)
            .Include(meeting =>
                meeting.Attachments)
            .FirstOrDefaultAsync(meeting =>
                meeting.MeetingId == id);

        if (meeting is null)
        {
            return null;
        }

        var dto =
            _mapper.Map<MeetingDetailDTO>(
                meeting);

        var organizer =
            await _context.Participants
                .AsNoTracking()
                .FirstOrDefaultAsync(participant =>
                    participant.ApplicationUserId ==
                    meeting.OrganizerId);

        if (organizer is not null)
        {
            dto.Organizer =
                new MeetingOrganizerDTO(
                    organizer.ParticipantId,
                    organizer.FirstName,
                    organizer.LastName);
        }

        dto.Attachments = dto.Attachments
            .Select(attachment => attachment with
            {
                DownloadUrl =
                $"/api/meetings/{meeting.MeetingId}/attachments/" +
                $"{attachment.Id}/download"
            })
            .ToList();

        return dto;
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
    /// Виникає, якщо кімнату або одного
    /// з указаних учасників не знайдено.
    /// </exception>
    public async Task<MeetingReadDTO> CreateAsync(
        MeetingCreateDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var room = await GetRoomAsync(
            dto.RoomNumber);

        var participantIds =
            dto.ParticipantIds
                .Distinct()
                .ToList();

        await ValidateParticipantIdsAsync(
            participantIds);

        var meeting =
            _mapper.Map<Meeting>(dto);

        meeting.RoomId =
            room?.RoomId;

        meeting.MeetingParticipants =
            participantIds
                .Select(participantId =>
                    new MeetingParticipant
                    {
                        ParticipantId =
                            participantId
                    })
                .ToList();

        _context.Meetings.Add(
            meeting);

        await _context.SaveChangesAsync();

        var createdMeeting =
            await _context.Meetings
                .AsNoTracking()
                .Include(item =>
                    item.Room)
                .Include(item =>
                    item.MeetingParticipants)
                .FirstAsync(item =>
                    item.MeetingId ==
                    meeting.MeetingId);

        return _mapper.Map<MeetingReadDTO>(
            createdMeeting);
    }

    /// <summary>
    /// Повністю оновлює існуючу зустріч.
    /// </summary>
    /// <param name="id">
    /// Ідентифікатор зустрічі.
    /// </param>
    /// <param name="dto">
    /// Нові дані зустрічі.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, якщо зустріч оновлено;
    /// інакше <see langword="false"/>.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Виникає, якщо кімнату або одного
    /// з указаних учасників не знайдено.
    /// </exception>
    public async Task<bool> UpdateAsync(
        int id,
        MeetingUpdateDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var meeting =
            await _context.Meetings
                .Include(item =>
                    item.MeetingParticipants)
                .FirstOrDefaultAsync(item =>
                    item.MeetingId == id);

        if (meeting is null)
        {
            return false;
        }

        var room = await GetRoomAsync(
            dto.RoomNumber);

        var participantIds =
            dto.ParticipantIds
                .Distinct()
                .ToList();

        await ValidateParticipantIdsAsync(
            participantIds);

        meeting.Title =
            dto.Title;

        meeting.Description =
            dto.Description;

        meeting.DateTime =
            dto.DateTime;

        meeting.RoomId =
            room?.RoomId;

        meeting.MeetingParticipants.Clear();

        foreach (var participantId
                 in participantIds)
        {
            meeting.MeetingParticipants.Add(
                new MeetingParticipant
                {
                    MeetingId =
                        meeting.MeetingId,

                    ParticipantId =
                        participantId
                });
        }

        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Частково оновлює існуючу зустріч.
    /// Змінюються лише передані поля.
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
    /// Виникає, якщо кімнату із зазначеним
    /// номером не знайдено.
    /// </exception>
    public async Task<bool> PartialUpdateAsync(
        int id,
        MeetingPartialUpdateDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var meeting =
            await _context.Meetings
                .FirstOrDefaultAsync(item =>
                    item.MeetingId == id);

        if (meeting is null)
        {
            return false;
        }

        Room? room = null;

        if (dto.RoomNumber.HasValue)
        {
            room = await GetRoomAsync(
                dto.RoomNumber);
        }

        if (dto.Title is not null)
        {
            meeting.Title =
                dto.Title;
        }

        if (dto.Description is not null)
        {
            meeting.Description =
                dto.Description;
        }

        if (dto.DateTime.HasValue)
        {
            meeting.DateTime =
                dto.DateTime.Value;
        }

        if (room is not null)
        {
            meeting.RoomId =
                room.RoomId;
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
    public async Task<bool> DeleteAsync(
        int id)
    {
        var meeting =
            await _context.Meetings
                .FirstOrDefaultAsync(item =>
                    item.MeetingId == id);

        if (meeting is null)
        {
            return false;
        }

        _context.Meetings.Remove(
            meeting);

        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Видаляє декілька зустрічей
    /// за списком ідентифікаторів.
    /// </summary>
    /// <param name="ids">
    /// Список ідентифікаторів зустрічей.
    /// </param>
    /// <returns>
    /// Кількість фактично видалених зустрічей.
    /// </returns>
    public async Task<int> DeleteManyAsync(
        List<int> ids)
    {
        ArgumentNullException.ThrowIfNull(ids);

        var validIds = ids
            .Where(id =>
                id > 0)
            .Distinct()
            .ToList();

        if (validIds.Count == 0)
        {
            return 0;
        }

        var meetings =
            await _context.Meetings
                .Where(meeting =>
                    validIds.Contains(
                        meeting.MeetingId))
                .ToListAsync();

        if (meetings.Count == 0)
        {
            return 0;
        }

        _context.Meetings.RemoveRange(
            meetings);

        await _context.SaveChangesAsync();

        return meetings.Count;
    }

    /// <summary>
    /// Отримує всі зустрічі,
    /// у яких бере участь зазначений учасник.
    /// </summary>
    /// <param name="participantId">
    /// Ідентифікатор учасника.
    /// </param>
    /// <returns>
    /// Список зустрічей учасника.
    /// Якщо зустрічей немає,
    /// повертається порожній список.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Виникає, якщо ідентифікатор
    /// учасника некоректний.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// Виникає, якщо учасника із зазначеним
    /// ідентифікатором не знайдено.
    /// </exception>
    public async Task<List<MeetingReadDTO>>
        GetByParticipantAsync(
            int participantId)
    {
        if (participantId <= 0)
        {
            throw new ValidationException(
                nameof(participantId),
                "Ідентифікатор учасника повинен бути більшим за нуль.");
        }

        var participantExists =
            await _context.Participants
                .AsNoTracking()
                .AnyAsync(participant =>
                    participant.ParticipantId ==
                    participantId);

        if (!participantExists)
        {
            throw new KeyNotFoundException(
                $"Учасника з ідентифікатором " +
                $"{participantId} не знайдено.");
        }

        var meetings =
            await _context.Meetings
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

        return _mapper.Map<List<MeetingReadDTO>>(
            meetings);
    }

    /// <summary>
    /// Повертає кімнату за її номером.
    /// </summary>
    /// <param name="roomNumber">
    /// Номер кімнати.
    /// </param>
    /// <returns>
    /// Знайдена кімната або
    /// <see langword="null"/>, якщо номер не передано.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Виникає, якщо кімнату
    /// із зазначеним номером не знайдено.
    /// </exception>
    private async Task<Room?> GetRoomAsync(
        int? roomNumber)
    {
        if (!roomNumber.HasValue)
        {
            return null;
        }

        var room =
            await _context.Rooms
                .AsNoTracking()
                .FirstOrDefaultAsync(item =>
                    item.NumberRoom ==
                    roomNumber.Value);

        if (room is null)
        {
            throw new ValidationException(
                nameof(roomNumber),
                "Кімнату із зазначеним номером не знайдено.");
        }

        return room;
    }

    /// <summary>
    /// Перевіряє існування всіх учасників,
    /// переданих у списку ідентифікаторів.
    /// </summary>
    /// <param name="participantIds">
    /// Список ідентифікаторів учасників.
    /// </param>
    /// <exception cref="ValidationException">
    /// Виникає, якщо одного або декількох
    /// учасників не знайдено.
    /// </exception>
    private async Task ValidateParticipantIdsAsync(
        List<int> participantIds)
    {
        var existingParticipantIds =
            await _context.Participants
                .AsNoTracking()
                .Where(participant =>
                    participantIds.Contains(
                        participant.ParticipantId))
                .Select(participant =>
                    participant.ParticipantId)
                .ToListAsync();

        var missingParticipantIds =
            participantIds
                .Except(existingParticipantIds)
                .ToList();

        if (missingParticipantIds.Count == 0)
        {
            return;
        }

        throw new ValidationException(
            nameof(participantIds),
            $"Не знайдено учасників з ідентифікаторами: " +
            $"{string.Join(", ", missingParticipantIds)}.");
    }
}