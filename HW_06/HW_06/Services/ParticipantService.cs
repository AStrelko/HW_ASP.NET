using AutoMapper;
using HW_06.DTOs.MeetingDTO;
using HW_06.DTOs.ParticipantDTO;
using HW_06.DTOs.Participants;
using HW_06.Helpers.Pagination;
using HW_06.Helpers.Queryable;
using HW_06.Helpers.QueryParameters;
using HW_06.Models;
using HW_06.Models.Files;
using HW_06.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using AppValidationException =
    HW_06.Validators.Exceptions.ValidationException;

namespace HW_06.Services;

/// <summary>
/// Сервіс для роботи з учасниками зустрічей.
/// Виконує отримання, оновлення та видалення учасників,
/// роботу з їх зустрічами, аватарами
/// та пов'язаними обліковими записами Identity.
/// </summary>
public class ParticipantService : IParticipantService
{
    /// <summary>
    /// Каталог для зберігання файлів аватарів.
    /// </summary>
    private const string AvatarFolder = "Avatars";

    /// <summary>
    /// Контекст бази даних застосунку.
    /// </summary>
    private readonly DataContext _context;

    /// <summary>
    /// Сервіс AutoMapper для перетворення
    /// моделей домену в DTO.
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    /// Сервіс для роботи з файловим сховищем.
    /// </summary>
    private readonly IFileStorageService _fileStorageService;

    /// <summary>
    /// Менеджер користувачів ASP.NET Core Identity.
    /// </summary>
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Ініціалізує сервіс для роботи з учасниками.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="mapper">
    /// Сервіс AutoMapper.
    /// </param>
    /// <param name="fileStorageService">
    /// Сервіс файлового сховища.
    /// </param>
    /// <param name="userManager">
    /// Менеджер користувачів ASP.NET Core Identity.
    /// </param>
    public ParticipantService(
        DataContext context,
        IMapper mapper,
        IFileStorageService fileStorageService,
        UserManager<ApplicationUser> userManager)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(fileStorageService);
        ArgumentNullException.ThrowIfNull(userManager);

        _context = context;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
        _userManager = userManager;
    }

    /// <summary>
    /// Отримує список учасників із підтримкою
    /// пошуку за прізвищем, сортування та пагінації.
    /// </summary>
    /// <param name="parameters">
    /// Параметри пошуку, сортування та пагінації.
    /// </param>
    /// <returns>
    /// Сторінка учасників разом з інформацією
    /// про пагінацію.
    /// </returns>
    public async Task<PagedResult<ParticipantReadDTO>> GetAllAsync(ParticipantQueryParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
    
        IQueryable<Participant> query =
            _context.Participants
                .AsNoTracking()
                .Include(participant =>
                    participant.ApplicationUser);
    
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
/// разом із його обліковим записом,
/// зустрічами та приватними файлами.
/// </summary>
/// <param name="id">
/// Ідентифікатор учасника.
/// </param>
/// <returns>
/// Детальна інформація про учасника або
/// <see langword="null"/>, якщо учасника не знайдено.
/// </returns>
public async Task<ParticipantDetailDTO?> GetByIdAsync(int id)
{
    var participant = await _context.Participants
        .AsNoTracking()
        .Include(participant =>
            participant.ApplicationUser)
        .Include(participant =>
            participant.MeetingParticipants)
        .ThenInclude(meetingParticipant =>
            meetingParticipant.Meeting)
        .ThenInclude(meeting =>
            meeting.Room)
        .Include(participant =>
            participant.SentPrivateFiles)
        .ThenInclude(file =>
            file.RecipientParticipant)
        .Include(participant =>
            participant.ReceivedPrivateFiles)
        .ThenInclude(file =>
            file.SenderParticipant)
        .FirstOrDefaultAsync(participant =>
            participant.ParticipantId == id);

    if (participant is null)
    {
        return null;
    }

    // Встановлює відправника для надісланих файлів,
    // оскільки ним є поточний учасник.
    foreach (var file in participant.SentPrivateFiles)
    {
        file.SenderParticipant = participant;
    }

    // Встановлює отримувача для отриманих файлів,
    // оскільки ним є поточний учасник.
    foreach (var file in participant.ReceivedPrivateFiles)
    {
        file.RecipientParticipant = participant;
    }

    var result =
        _mapper.Map<ParticipantDetailDTO>(participant);

    // Отримує ідентифікатори зустрічей учасника
    // для подальшого підрахунку кількості учасників.
    var meetingIds = participant.MeetingParticipants
        .Select(meetingParticipant =>
            meetingParticipant.MeetingId)
        .ToList();

    if (meetingIds.Count > 0)
    {
        var participantCounts =
            await _context.MeetingParticipants
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
    }

    // Формує URL для завантаження
    // надісланих приватних файлів.
    result.SentPrivateFiles =
        result.SentPrivateFiles
            .Select(file => file with
            {
                DownloadUrl =
                    $"/api/participants/{participant.ParticipantId}" +
                    $"/private-files/{file.Id}/download"
            })
            .ToList();

    // Формує URL для завантаження
    // отриманих приватних файлів.
    result.ReceivedPrivateFiles =
        result.ReceivedPrivateFiles
            .Select(file => file with
            {
                DownloadUrl =
                    $"/api/participants/{participant.ParticipantId}" +
                    $"/private-files/{file.Id}/download"
            })
            .ToList();

    return result;
}
    
    /// <summary>
    /// Повністю оновлює дані учасника
    /// та список його зустрічей.
    /// </summary>
    /// <param name="id">
    /// Ідентифікатор учасника.
    /// </param>
    /// <param name="dto">
    /// Нові дані учасника.
    /// </param>
    /// <returns>
    /// true, якщо учасника оновлено;
    /// false, якщо учасника не знайдено.
    /// </returns>
    public async Task<bool> UpdateAsync(
        int id,
        ParticipantUpdateDTO dto)
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

        var meetingIds = dto.MeetingIds
            .Distinct()
            .ToList();

        await ValidateMeetingIdsAsync(
            meetingIds);

        participant.FirstName =
            dto.FirstName.Trim();

        participant.LastName =
            dto.LastName.Trim();

        participant.Position =
            dto.Position?.Trim();

        participant.MeetingParticipants.Clear();

        foreach (var meetingId in meetingIds)
        {
            participant.MeetingParticipants.Add(
                new MeetingParticipant
                {
                    ParticipantId =
                        participant.ParticipantId,

                    MeetingId =
                        meetingId
                });
        }

        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Частково оновлює дані учасника
    /// та, за необхідності, список його зустрічей.
    /// </summary>
    /// <param name="id">
    /// Ідентифікатор учасника.
    /// </param>
    /// <param name="dto">
    /// Поля учасника, які необхідно оновити.
    /// </param>
    /// <returns>
    /// true, якщо учасника оновлено;
    /// false, якщо учасника не знайдено.
    /// </returns>
    public async Task<bool> PartialUpdateAsync(
        int id,
        ParticipantPartialUpdateDTO dto)
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

        if (dto.Position is not null)
        {
            participant.Position =
                dto.Position.Trim();
        }

        if (dto.MeetingIds is not null)
        {
            var meetingIds = dto.MeetingIds
                .Distinct()
                .ToList();

            await ValidateMeetingIdsAsync(
                meetingIds);

            participant.MeetingParticipants.Clear();

            foreach (var meetingId in meetingIds)
            {
                participant.MeetingParticipants.Add(
                    new MeetingParticipant
                    {
                        ParticipantId =
                            participant.ParticipantId,

                        MeetingId =
                            meetingId
                    });
            }
        }

        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Видаляє учасника, його зв’язки із зустрічами,
    /// обліковий запис Identity та файл аватара.
    /// </summary>
    /// <param name="id">
    /// Ідентифікатор учасника.
    /// </param>
    /// <returns>
    /// true, якщо учасника видалено;
    /// false, якщо учасника не знайдено.
    /// </returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var participant = await _context.Participants
            .Include(participant =>
                participant.MeetingParticipants)
            .Include(participant =>
                participant.ApplicationUser)
            .FirstOrDefaultAsync(participant =>
                participant.ParticipantId == id);

        if (participant is null)
        {
            return false;
        }

        var avatarFileName =
            participant.AvatarFileName;

        var applicationUser =
            participant.ApplicationUser;

        _context.MeetingParticipants.RemoveRange(
            participant.MeetingParticipants);

        _context.Participants.Remove(
            participant);

        await _context.SaveChangesAsync();

        if (applicationUser is not null)
        {
            var identityResult =
                await _userManager.DeleteAsync(
                    applicationUser);

            if (!identityResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Не вдалося видалити обліковий запис користувача.");
            }
        }

        if (!string.IsNullOrWhiteSpace(
                avatarFileName))
        {
            await _fileStorageService.DeleteAsync(
                AvatarFolder,
                avatarFileName,
                FileAccessLevel.Public);
        }

        return true;
    }

    /// <summary>
    /// Видаляє декількох учасників,
    /// їхні зв’язки із зустрічами,
    /// облікові записи Identity та файли аватарів.
    /// </summary>
    /// <param name="ids">
    /// Список ідентифікаторів учасників.
    /// </param>
    /// <returns>
    /// Кількість видалених учасників.
    /// </returns>
    public async Task<int> DeleteManyAsync(
        List<int> ids)
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
            .Include(participant =>
                participant.ApplicationUser)
            .Where(participant =>
                participantIds.Contains(
                    participant.ParticipantId))
            .ToListAsync();
    
        if (participants.Count == 0)
        {
            return 0;
        }
    
        var avatarFileNames = participants
            .Where(participant =>
                !string.IsNullOrWhiteSpace(
                    participant.AvatarFileName))
            .Select(participant =>
                participant.AvatarFileName!)
            .Distinct()
            .ToList();
    
        var applicationUsers = participants
            .Where(participant =>
                participant.ApplicationUser is not null)
            .Select(participant =>
                participant.ApplicationUser!)
            .ToList();
    
        var meetingParticipants = participants
            .SelectMany(participant =>
                participant.MeetingParticipants)
            .ToList();
    
        _context.MeetingParticipants.RemoveRange(
            meetingParticipants);
    
        _context.Participants.RemoveRange(
            participants);
    
        await _context.SaveChangesAsync();
    
        foreach (var applicationUser in applicationUsers)
        {
            var identityResult =
                await _userManager.DeleteAsync(
                    applicationUser);
    
            if (!identityResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Не вдалося видалити обліковий запис користувача " +
                    $"{applicationUser.Email}.");
            }
        }
    
        foreach (var avatarFileName in avatarFileNames)
        {
            await _fileStorageService.DeleteAsync(
                AvatarFolder,
                avatarFileName,
                FileAccessLevel.Public);
        }
    
        return participants.Count;
    }

    /// <summary>
    /// Отримує список зустрічей,
    /// у яких бере участь вказаний учасник.
    /// </summary>
    /// <param name="participantId">
    /// Ідентифікатор учасника.
    /// </param>
    /// <returns>
    /// Список зустрічей учасника,
    /// впорядкований за датою.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Виникає, якщо учасника із зазначеним
    /// ідентифікатором не знайдено.
    /// </exception>
    public async Task<List<MeetingReadDTO>> GetMeetingsAsync(
        int participantId)
    {
        var participantExists =
            await _context.Participants
                .AsNoTracking()
                .AnyAsync(participant =>
                    participant.ParticipantId ==
                    participantId);

        if (!participantExists)
        {
            throw new KeyNotFoundException(
                $"Учасника з ідентифікатором {participantId} не знайдено.");
        }

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
            _mapper.Map<List<MeetingReadDTO>>(
                meetings);

        var participantCounts = meetings
            .ToDictionary(
                meeting => meeting.MeetingId,
                meeting =>
                    meeting.MeetingParticipants.Count);

        foreach (var meeting in result)
        {
            meeting.ParticipantsCount =
                participantCounts.GetValueOrDefault(
                    meeting.MeetingId);
        }

        return result;
    }
    
    /// <summary>
    /// Отримує коротку інформацію
    /// про учасника разом із даними аватара.
    /// </summary>
    /// <param name="participantId">
    /// Ідентифікатор учасника.
    /// </param>
    /// <returns>
    /// DTO учасника з даними аватара
    /// або null, якщо учасника не знайдено.
    /// </returns>
    public async Task<ParticipantAvatarDTO?> GetAvatarAsync(
        int participantId)
    {
        return await _context.Participants
            .AsNoTracking()
            .Where(participant =>
                participant.ParticipantId ==
                participantId)
            .Select(participant =>
                new ParticipantAvatarDTO
                {
                    ParticipantId =
                        participant.ParticipantId,

                    FirstName =
                        participant.FirstName,

                    LastName =
                        participant.LastName,

                    AvatarFileName =
                        participant.AvatarFileName,

                    AvatarUrl =
                        null
                })
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Додає або замінює аватар учасника.
    /// </summary>
    /// <param name="participantId">
    /// Ідентифікатор учасника.
    /// </param>
    /// <param name="file">
    /// Файл нового аватара.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
    /// <returns>
    /// DTO учасника з даними аватара
    /// або null, якщо учасника не знайдено.
    /// </returns>
    public async Task<ParticipantAvatarDTO?> UploadAvatarAsync(
        int participantId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
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

        participant.AvatarFileName =
            savedFileName;

        await _context.SaveChangesAsync(
            cancellationToken);

        return new ParticipantAvatarDTO
        {
            ParticipantId =
                participant.ParticipantId,

            FirstName =
                participant.FirstName,

            LastName =
                participant.LastName,

            AvatarFileName =
                participant.AvatarFileName,

            AvatarUrl =
                null
        };
    }
    
    /// <summary>
    /// Видаляє власний аватар учасника
    /// та повертає використання стандартного аватара.
    /// </summary>
    /// <param name="participantId">
    /// Ідентифікатор учасника.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
    /// <returns>
    /// true, якщо учасника знайдено та аватар скинуто;
    /// false, якщо учасника не знайдено.
    /// </returns>
    public async Task<bool> ResetAvatarAsync(
        int participantId,
        CancellationToken cancellationToken = default)
    {
        var participant = await _context.Participants
            .FirstOrDefaultAsync(
                participant =>
                    participant.ParticipantId == participantId,
                cancellationToken);

        if (participant is null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(
                participant.AvatarFileName))
        {
            await _fileStorageService.DeleteAsync(
                AvatarFolder,
                participant.AvatarFileName,
                FileAccessLevel.Public,
                cancellationToken);
        }

        participant.AvatarFileName = null;

        await _context.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    /// <summary>
    /// Перевіряє, що всі передані
    /// ідентифікатори зустрічей існують.
    /// </summary>
    /// <param name="meetingIds">
    /// Список ідентифікаторів зустрічей.
    /// </param>
    /// <exception cref="AppValidationException">
    /// Виникає, якщо одну або декілька
    /// зустрічей не знайдено.
    /// </exception>
    private async Task ValidateMeetingIdsAsync(
        List<int> meetingIds)
    {
        if (meetingIds.Count == 0)
        {
            return;
        }

        var existingMeetingIds =
            await _context.Meetings
                .AsNoTracking()
                .Where(meeting =>
                    meetingIds.Contains(
                        meeting.MeetingId))
                .Select(meeting =>
                    meeting.MeetingId)
                .ToListAsync();

        var missingMeetingIds = meetingIds
            .Except(existingMeetingIds)
            .ToList();

        if (missingMeetingIds.Count > 0)
        {
            throw new AppValidationException(
                "MeetingIds",
                $"Зустрічі не знайдено: " +
                $"{string.Join(", ", missingMeetingIds)}.");
        }
    }
    
    public async Task<int?> GetParticipantIdByUserIdAsync(
        string applicationUserId)
    {
        return await _context.Participants
            .AsNoTracking()
            .Where(participant =>
                participant.ApplicationUserId ==
                applicationUserId)
            .Select(participant =>
                (int?)participant.ParticipantId)
            .FirstOrDefaultAsync();
    }
}