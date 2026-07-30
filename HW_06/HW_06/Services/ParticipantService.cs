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
using HW_06.DTOs.Participants;
using HW_06.Models.Files;


namespace HW_06.Services;

public class ParticipantService : IParticipantService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private const string AvatarFolder = "Avatars";

    private readonly IFileStorageService _fileStorageService;

    private readonly IValidator<ParticipantCreateDTO>
        _createValidator;

    private readonly IValidator<ParticipantUpdateDTO>
        _updateValidator;

    private readonly IValidator<ParticipantPartialUpdateDTO>
        _partialValidator;

    /// <summary>
    /// Ініціалізує сервіс для роботи з учасниками.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="mapper">
    /// Екземпляр AutoMapper.
    /// </param>
    /// <param name="createValidator">
    /// Валідатор створення учасника.
    /// </param>
    /// <param name="updateValidator">
    /// Валідатор повного оновлення учасника.
    /// </param>
    /// <param name="partialValidator">
    /// Валідатор часткового оновлення учасника.
    /// </param>
    /// <param name="fileStorageService">
    /// Сервіс локального файлового сховища.
    /// </param>
    public ParticipantService(
        DataContext context,
        IMapper mapper,
        IValidator<ParticipantCreateDTO> createValidator,
        IValidator<ParticipantUpdateDTO> updateValidator,
        IValidator<ParticipantPartialUpdateDTO> partialValidator,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _partialValidator = partialValidator;
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// Отримує список учасників із підтримкою
    /// пошуку за прізвищем, сортування та пагінації.
    /// </summary>
    /// <param name="parameters">
    /// Параметри пошуку, сортування та пагінації учасників.
    /// </param>
    /// <returns>
    /// Сторінку учасників разом із загальною кількістю записів
    /// та інформацією про пагінацію.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Виникає, якщо параметри запиту не передано.
    /// </exception>
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

    /// <summary>
    /// Отримує детальну інформацію про учасника
    /// разом зі списком його зустрічей.
    /// </summary>
    /// <param name="id">
    /// Унікальний ідентифікатор учасника.
    /// </param>
    /// <returns>
    /// Детальну інформацію про учасника та його зустрічі або
    /// <see langword="null"/>, якщо учасника не знайдено.
    /// </returns>
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

    /// <summary>
    /// Створює нового учасника та додає йому аватар,
    /// якщо файл аватара був переданий.
    /// </summary>
    /// <param name="dto">
    /// Дані нового учасника, список зустрічей
    /// та необов’язковий файл аватара.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
    /// <returns>
    /// Створений учасник.
    /// </returns>
    public async Task<ParticipantReadDTO> CreateAsync(
        ParticipantCreateDTO dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult =
            _createValidator.Validate(dto);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(
                validationResult.Errors);
        }

        await ValidateEmailUniquenessAsync(
            dto.Email,
            cancellationToken: cancellationToken);

        var meetingIds = dto.MeetingIds
            .Distinct()
            .ToList();

        await ValidateMeetingIdsAsync(
            meetingIds,
            nameof(dto.MeetingIds));

        var participant =
            _mapper.Map<Participant>(dto);

        string? savedAvatarFileName = null;

        try
        {
            if (dto.Avatar is not null &&
                dto.Avatar.Length > 0)
            {
                savedAvatarFileName =
                    await _fileStorageService.SaveAsync(
                        dto.Avatar,
                        AvatarFolder,
                        FileAccessLevel.Public,
                        cancellationToken);

                participant.AvatarFileName =
                    savedAvatarFileName;
            }

            participant.MeetingParticipants =
                meetingIds
                    .Select(meetingId =>
                        new MeetingParticipant
                        {
                            MeetingId = meetingId
                        })
                    .ToList();

            await _context.Participants.AddAsync(
                participant,
                cancellationToken);

            await _context.SaveChangesAsync(
                cancellationToken);

            return _mapper.Map<ParticipantReadDTO>(
                participant);
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(
                    savedAvatarFileName))
            {
                await _fileStorageService.DeleteAsync(
                    AvatarFolder,
                    savedAvatarFileName,
                    FileAccessLevel.Public,
                    cancellationToken);
            }

            throw;
        }
    }
    /// <summary>
    /// Перевіряє унікальність адреси електронної пошти учасника.
    /// </summary>
    /// <param name="email">
    /// Адреса електронної пошти, яку необхідно перевірити.
    /// </param>
    /// <param name="excludedParticipantId">
    /// Ідентифікатор учасника, якого необхідно виключити
    /// з перевірки під час оновлення.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
    /// <exception cref="ValidationException">
    /// Виникає, якщо адреса електронної пошти
    /// вже використовується іншим учасником.
    /// </exception>
    private async Task ValidateEmailUniquenessAsync(
        string email,
        int? excludedParticipantId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email
            .Trim()
            .ToLowerInvariant();

        var emailExists = await _context.Participants
            .AnyAsync(
                participant =>
                    participant.Email.ToLower() ==
                    normalizedEmail &&
                    (!excludedParticipantId.HasValue ||
                     participant.ParticipantId !=
                     excludedParticipantId.Value),
                cancellationToken);

        if (emailExists)
        {
            throw new ValidationException(
                "Email",
                "Учасник із такою електронною поштою вже існує.");
        }
    }

    /// <summary>
    /// Повністю оновлює дані учасника та список його зустрічей.
    /// </summary>
    /// <param name="id">
    /// Унікальний ідентифікатор учасника.
    /// </param>
    /// <param name="dto">
    /// Нові дані учасника та список ідентифікаторів зустрічей.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, якщо учасника успішно оновлено;
    /// <see langword="false"/>, якщо учасника не знайдено.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Виникає, якщо передані дані не пройшли валідацію,
    /// електронна пошта вже використовується іншим учасником
    /// або вказані зустрічі не існують.
    /// </exception>
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

        await ValidateEmailUniquenessAsync(
            dto.Email,
            id);

        var normalizedEmail = dto.Email
            .Trim()
            .ToLowerInvariant();

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

    /// <summary>
    /// Частково оновлює дані учасника та, за необхідності,
    /// список його зустрічей.
    /// </summary>
    /// <param name="id">
    /// Унікальний ідентифікатор учасника.
    /// </param>
    /// <param name="dto">
    /// Дані полів учасника, які необхідно оновити.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, якщо учасника успішно оновлено;
    /// <see langword="false"/>, якщо учасника не знайдено.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Виникає, якщо передані дані не пройшли валідацію,
    /// електронна пошта вже використовується іншим учасником
    /// або вказані зустрічі не існують.
    /// </exception>
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
            await ValidateEmailUniquenessAsync(
                dto.Email,
                id);

            participant.Email = dto.Email
                .Trim()
                .ToLowerInvariant();
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

    /// <summary>
    /// Видаляє учасника та його зв’язки із зустрічами.
    /// </summary>
    /// <param name="id">
    /// Унікальний ідентифікатор учасника.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, якщо учасника успішно видалено;
    /// <see langword="false"/>, якщо учасника не знайдено.
    /// </returns>
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

    /// <summary>
    /// Видаляє декількох учасників та їхні зв’язки із зустрічами.
    /// </summary>
    /// <param name="ids">
    /// Список ідентифікаторів учасників, яких необхідно видалити.
    /// </param>
    /// <returns>
    /// Кількість фактично видалених учасників.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Виникає, якщо список ідентифікаторів не передано.
    /// </exception>
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

    /// <summary>
    /// Отримує список зустрічей, у яких бере участь
    /// вказаний учасник.
    /// </summary>
    /// <param name="participantId">
    /// Унікальний ідентифікатор учасника.
    /// </param>
    /// <returns>
    /// Список зустрічей учасника, впорядкований за датою.
    /// Якщо зустрічей не знайдено, повертається порожній список.
    /// </returns>
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
    
    /// <summary>
    /// Отримує коротку інформацію про учасника
    /// разом із даними його аватара.
    /// </summary>
    /// <param name="participantId">
    /// Унікальний ідентифікатор учасника.
    /// </param>
    /// <returns>
    /// DTO учасника з даними аватара або
    /// <see langword="null"/>, якщо учасника не знайдено.
    /// </returns>
    public async Task<ParticipantAvatarDTO?> GetAvatarAsync(
        int participantId)
    {
        return await _context.Participants
            .AsNoTracking()
            .Where(participant =>
                participant.ParticipantId == participantId)
            .Select(participant =>
                new ParticipantAvatarDTO
                {
                    ParticipantId = participant.ParticipantId,
                    FirstName = participant.FirstName,
                    LastName = participant.LastName,
                    AvatarFileName = participant.AvatarFileName,
                    AvatarUrl = null
                })
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Додає або замінює аватар учасника.
    /// </summary>
    /// <param name="participantId">
    /// Унікальний ідентифікатор учасника.
    /// </param>
    /// <param name="file">
    /// Файл нового аватара.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
    /// <returns>
    /// DTO учасника з даними доданого або оновленого аватара,
    /// або <see langword="null"/>, якщо учасника не знайдено.
    /// </returns>
    /// <exception cref="ValidationException">
    /// Виникає, якщо файл не передано або він порожній.
    /// </exception>
    public async Task<ParticipantAvatarDTO?> UploadAvatarAsync(
        int participantId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            throw new ValidationException(
                nameof(file),
                "Файл аватара не передано або він порожній.");
        }

        var participant = await _context.Participants
            .FirstOrDefaultAsync(
                participant =>
                    participant.ParticipantId == participantId,
                cancellationToken);

        if (participant is null)
        {
            return null;
        }

        string savedFileName;

        if (!string.IsNullOrWhiteSpace(
                participant.AvatarFileName))
        {
            savedFileName =
                await _fileStorageService.ReplaceAsync(
                    file,
                    AvatarFolder,
                    participant.AvatarFileName,
                    FileAccessLevel.Public,
                    cancellationToken);
        }
        else
        {
            savedFileName =
                await _fileStorageService.SaveAsync(
                    file,
                    AvatarFolder,
                    FileAccessLevel.Public,
                    cancellationToken);
        }

        participant.AvatarFileName = savedFileName;

        await _context.SaveChangesAsync(
            cancellationToken);

        return new ParticipantAvatarDTO
        {
            ParticipantId = participant.ParticipantId,
            FirstName = participant.FirstName,
            LastName = participant.LastName,
            AvatarFileName = participant.AvatarFileName,
            AvatarUrl = null
        };
    }
    
    /// <summary>
    /// Перевіряє, що всі передані ідентифікатори зустрічей існують
    /// в базі даних.
    /// </summary>
    /// <param name="meetingIds">
    /// Список ідентифікаторів зустрічей.
    /// </param>
    /// <param name="propertyName">
    /// Назва властивості, для якої формується повідомлення
    /// про помилку валідації.
    /// </param>
    /// <exception cref="ValidationException">
    /// Виникає, якщо хоча б одну із зустрічей не знайдено.
    /// </exception>
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