using AutoMapper;
using HW_06.DTOs.Files;
using HW_06.Models;
using HW_06.Services.Interfaces;
using HW_06.Services.Results;
using HW_06.Validators.FileValid;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Services;

/// <summary>
/// Реалізує операції для роботи
/// з приватними файлами учасників.
/// </summary>
public class PrivateAttachmentService : IPrivateAttachmentService
{
    /// <summary>
    /// Контекст бази даних застосунку.
    /// </summary>
    private readonly DataContext _context;

    /// <summary>
    /// Сервіс AutoMapper для перетворення
    /// сутностей у DTO.
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    /// Інформація про середовище
    /// виконання застосунку.
    /// </summary>
    private readonly IWebHostEnvironment _environment;

    

    /// <summary>
    /// Ініціалізує новий екземпляр сервісу
    /// приватних файлів учасників.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="mapper">
    /// Сервіс перетворення моделей у DTO.
    /// </param>
    /// <param name="environment">
    /// Інформація про середовище виконання застосунку.
    /// </param>
    public PrivateAttachmentService(
        DataContext context,
        IMapper mapper,
        IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(environment);

        _context = context;
        _mapper = mapper;
        _environment = environment;
    }

    /// <summary>
    /// Завантажує приватний документ
    /// від одного учасника іншому.
    /// </summary>
    /// <param name="senderParticipantId">
    /// Ідентифікатор учасника-відправника.
    /// </param>
    /// <param name="recipientParticipantId">
    /// Ідентифікатор учасника-отримувача.
    /// </param>
    /// <param name="file">
    /// Документ, який необхідно передати.
    /// </param>
    /// <returns>
    /// Дані завантаженого документа або
    /// <see langword="null"/>, якщо одного з учасників не знайдено.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Виникає, якщо документ не пройшов перевірку
    /// або відправник і отримувач збігаються.
    /// </exception>
    public async Task<AttachmentPrivateDTO?> UploadAsync(
    int senderParticipantId,
    int recipientParticipantId,
    IFormFile file)
{
    if (senderParticipantId <= 0)
    {
        throw new ArgumentException(
            "Ідентифікатор учасника-відправника повинен бути більшим за нуль.");
    }

    if (recipientParticipantId <= 0)
    {
        throw new ArgumentException(
            "Ідентифікатор учасника-отримувача повинен бути більшим за нуль.");
    }

    if (senderParticipantId == recipientParticipantId)
    {
        throw new ArgumentException(
            "Учасник не може надіслати приватний файл самому собі.");
    }
    
    // Перевіряє, що учасник не надсилає документ самому собі.
    if (senderParticipantId == recipientParticipantId)
    {
        throw new ArgumentException(
            "Учасник не може надіслати приватний файл самому собі.");
    }

    // Перевіряє існування учасника-відправника.
    var sender = await _context.Participants
        .AsNoTracking()
        .FirstOrDefaultAsync(participant =>
            participant.ParticipantId == senderParticipantId);

    if (sender is null)
    {
        throw new KeyNotFoundException(
            $"Учасника-відправника з ідентифікатором " +
            $"{senderParticipantId} не знайдено.");
    }

    // Перевіряє існування учасника-отримувача.
    var recipient = await _context.Participants
        .AsNoTracking()
        .FirstOrDefaultAsync(participant =>
            participant.ParticipantId == recipientParticipantId);

    if (recipient is null)
    {
        throw new KeyNotFoundException(
            $"Учасника-отримувача з ідентифікатором " +
            $"{recipientParticipantId} не знайдено.");
    }

   // Виконує комплексну перевірку приватного документа.
   var validationError =
       await PrivateDocumentValidator.ValidateAsync(
           file);
   
   if (validationError is not null)
   {
       throw new ArgumentException(
           validationError,
           nameof(file));
   }

    // Отримує оригінальне ім'я та розширення документа.
    var originalFileName = Path.GetFileName(file.FileName);
    var extension = Path.GetExtension(originalFileName);

    // Формує унікальне ім'я для збереження документа.
    var storedFileName = $"{Guid.NewGuid():N}{extension}";

    // Визначає каталог для приватних документів.
    var privateFilesDirectory = Path.Combine(
        _environment.ContentRootPath,
        "uploads",
        "PrivateFile",
        "Participants");

    // Створює каталог, якщо він ще не існує.
    Directory.CreateDirectory(privateFilesDirectory);

    var fullFilePath = Path.Combine(
        privateFilesDirectory,
        storedFileName);

    try
    {
        // Зберігає фізичний документ у локальному сховищі.
        await using (var fileStream = new FileStream(
                         fullFilePath,
                         FileMode.CreateNew,
                         FileAccess.Write,
                         FileShare.None))
        {
            await file.CopyToAsync(fileStream);
        }

        // Створює запис про приватний документ.
        var privateFile = new ParticipantPrivateFile
        {
            OriginalFileName = originalFileName,
            StoredFileName = storedFileName,
            ContentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? "application/octet-stream"
                : file.ContentType,
            SizeBytes = file.Length,
            UploadedAtUtc = DateTime.UtcNow,

            SenderParticipantId = senderParticipantId,
            RecipientParticipantId = recipientParticipantId
        };

        _context.ParticipantPrivateFiles.Add(privateFile);
        await _context.SaveChangesAsync();

        // Повторно завантажує документ разом із даними про відправника
        // та отримувача.
        var savedPrivateFile = await _context.ParticipantPrivateFiles
            .AsNoTracking()
            .Include(item => item.SenderParticipant)
            .Include(item => item.RecipientParticipant)
            .FirstAsync(item => item.Id == privateFile.Id);

        var dto = _mapper.Map<AttachmentPrivateDTO>(savedPrivateFile);

        // Формує посилання для завантаження документа.
        return dto with
        {
            DownloadUrl =
                $"/api/participants/{recipientParticipantId}" +
                $"/private-files/{savedPrivateFile.Id}/download"
        };
    }
    catch
    {
        // Видаляє фізичний документ, якщо збереження в базі даних завершилося помилкою.
        if (File.Exists(fullFilePath))
        {
            File.Delete(fullFilePath);
        }

        throw;
    }
}
    /// <summary>
    /// Повертає список приватних документів,
    /// отриманих указаним учасником.
    /// </summary>
    /// <param name="participantId">
    /// Ідентифікатор учасника.
    /// </param>
    /// <returns>
    /// Колекцію отриманих приватних документів.
    /// </returns>
    public async Task<IReadOnlyCollection<AttachmentPrivateDTO>>
        GetReceivedFilesAsync(int participantId)
    {
        if (participantId <= 0)
        {
            throw new ArgumentException(
                "Ідентифікатор учасника повинен бути більшим за нуль.");
        }
        
        var participantExists =
            await _context.Participants
                .AsNoTracking()
                .AnyAsync(participant =>
                    participant.ParticipantId == participantId);
        
        if (!participantExists)
        {
            throw new KeyNotFoundException(
                $"Учасника з ідентифікатором {participantId} не знайдено.");
        }
        var files = await _context.ParticipantPrivateFiles
            .AsNoTracking()
            .Include(file => file.SenderParticipant)
            .Include(file => file.RecipientParticipant)
            .Where(file =>
                file.RecipientParticipantId == participantId)
            .OrderByDescending(file => file.UploadedAtUtc)
            .ToListAsync();

        return files
            .Select(file =>
            {
                var dto = _mapper.Map<AttachmentPrivateDTO>(file);

                return dto with
                {
                    DownloadUrl =
                    $"/api/participants/{participantId}" +
                    $"/private-files/{file.Id}/download"
                };
            })
            .ToList();
    }

    /// <summary>
    /// Повертає список приватних документів,
    /// надісланих указаним учасником.
    /// </summary>
    /// <param name="participantId">
    /// Ідентифікатор учасника.
    /// </param>
    /// <returns>
    /// Колекцію надісланих приватних документів.
    /// </returns>
    public async Task<IReadOnlyCollection<AttachmentPrivateDTO>>
        GetSentFilesAsync(int participantId)
    {
        if (participantId <= 0)
        {
            throw new ArgumentException(
                "Ідентифікатор учасника повинен бути більшим за нуль.");
        }

        var participantExists =
            await _context.Participants
                .AsNoTracking()
                .AnyAsync(participant =>
                    participant.ParticipantId == participantId);

        if (!participantExists)
        {
            throw new KeyNotFoundException(
                $"Учасника з ідентифікатором {participantId} не знайдено.");
        }
        
        var files = await _context.ParticipantPrivateFiles
            .AsNoTracking()
            .Include(file => file.SenderParticipant)
            .Include(file => file.RecipientParticipant)
            .Where(file =>
                file.SenderParticipantId == participantId)
            .OrderByDescending(file => file.UploadedAtUtc)
            .ToListAsync();

        return files
            .Select(file =>
            {
                var dto = _mapper.Map<AttachmentPrivateDTO>(file);

                return dto with
                {
                    DownloadUrl =
                    $"/api/participants/{participantId}" +
                    $"/private-files/{file.Id}/download"
                };
            })
            .ToList();
    }

    /// <summary>
    /// Повертає інформацію
    /// про приватний документ.
    /// </summary>
    /// <param name="fileId">
    /// Ідентифікатор документа.
    /// </param>
    /// <param name="participantId">
    /// Ідентифікатор учасника,
    /// який запитує документ.
    /// </param>
    /// <returns>
    /// Інформацію про документ або
    /// <see langword="null"/>,
    /// якщо документ не знайдено
    /// або доступ заборонено.
    /// </returns>
    public async Task<AttachmentPrivateDTO?> GetByIdAsync(
        int fileId,
        int participantId)
    {
        var privateFile = await _context.ParticipantPrivateFiles
            .AsNoTracking()
            .Include(file => file.SenderParticipant)
            .Include(file => file.RecipientParticipant)
            .FirstOrDefaultAsync(file =>
                file.Id == fileId &&
                (
                    file.SenderParticipantId == participantId ||
                    file.RecipientParticipantId == participantId
                ));

        if (privateFile is null)
        {
            return null;
        }

        var dto = _mapper.Map<AttachmentPrivateDTO>(privateFile);

        return dto with
        {
            DownloadUrl =
            $"/api/participants/{participantId}" +
            $"/private-files/{privateFile.Id}/download"
        };
    }

    /// <summary>
    /// Повертає приватний документ
    /// для завантаження.
    /// </summary>
    /// <param name="fileId">
    /// Ідентифікатор документа.
    /// </param>
    /// <param name="participantId">
    /// Ідентифікатор учасника,
    /// який завантажує документ.
    /// </param>
    /// <returns>
    /// Дані документа для завантаження або
    /// <see langword="null"/>,
    /// якщо документ не знайдено
    /// або доступ до нього відсутній.
    /// </returns>
    public async Task<AttachmentDownloadResult?> DownloadAsync(
        int fileId,
        int participantId)
    {
        var privateFile = await _context.ParticipantPrivateFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(file =>
                file.Id == fileId &&
                (
                    file.SenderParticipantId == participantId ||
                    file.RecipientParticipantId == participantId
                ));

        if (privateFile is null)
        {
            return null;
        }

        var fullFilePath = Path.Combine(
            _environment.ContentRootPath,
            "uploads",
            "PrivateFile",
            "Participants",
            privateFile.StoredFileName);

        if (!File.Exists(fullFilePath))
        {
            return null;
        }

        var stream = new FileStream(
            fullFilePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        return new AttachmentDownloadResult(
            stream,
            privateFile.ContentType,
            privateFile.OriginalFileName);
    }

    /// <summary>
    /// Видаляє приватний документ.
    /// </summary>
    /// <param name="fileId">
    /// Ідентифікатор документа.
    /// </param>
    /// <param name="participantId">
    /// Ідентифікатор учасника,
    /// який видаляє документ.
    /// </param>
    /// <returns>
    /// <see langword="true"/>,
    /// якщо документ успішно видалено;
    /// інакше —
    /// <see langword="false"/>.
    /// </returns>
    public async Task<bool> DeleteAsync(
        int fileId,
        int participantId)
    {
        var privateFile = await _context.ParticipantPrivateFiles
            .FirstOrDefaultAsync(file =>
                file.Id == fileId &&
                file.SenderParticipantId == participantId);

        if (privateFile is null)
        {
            return false;
        }

        var fullFilePath = Path.Combine(
            _environment.ContentRootPath,
            "uploads",
            "PrivateFile",
            "Participants",
            privateFile.StoredFileName);

        _context.ParticipantPrivateFiles.Remove(privateFile);
        await _context.SaveChangesAsync();

        if (File.Exists(fullFilePath))
        {
            File.Delete(fullFilePath);
        }

        return true;
    }
}