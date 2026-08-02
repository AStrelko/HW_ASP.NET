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
/// з публічними файлами-вкладеннями зустрічей.
/// </summary>
public class AttachmentService : IAttachmentService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Ініціалізує новий екземпляр сервісу
    /// публічних файлів-вкладень зустрічей.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="mapper">
    /// Сервіс перетворення сутностей у DTO.
    /// </param>
    /// <param name="environment">
    /// Інформація про середовище виконання застосунку.
    /// </param>
    public AttachmentService(
        DataContext context,
        IMapper mapper,
        IWebHostEnvironment environment)
    {
        _context = context;
        _mapper = mapper;
        _environment = environment;
    }

    /// <summary>
    /// Завантажує публічний документ
    /// і прикріплює його до зустрічі.
    /// </summary>
    /// <param name="meetingId">
    /// Ідентифікатор зустрічі.
    /// </param>
    /// <param name="file">
    /// Документ, який необхідно завантажити.
    /// </param>
    /// <returns>
    /// Дані створеного вкладення або
    /// <see langword="null"/>, якщо зустріч не знайдено.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Виникає, якщо файл не пройшов валідацію.
    /// </exception>
    public async Task<AttachmentPublicDTO?> UploadAsync(
        int meetingId,
        IFormFile file)
    {
        // Перевіряє існування зустрічі.
        var meetingExists = await _context.Meetings
            .AnyAsync(meeting =>
                meeting.MeetingId == meetingId);

        if (!meetingExists)
        {
            return null;
        }

        // Перевіряє розмір, розширення, MIME-тип і сигнатуру документа.
        var validationError =
            await PublicDocumentValidator.ValidateAsync(file);

        if (validationError is not null)
        {
            throw new ArgumentException(
                validationError,
                nameof(file));
        }

        // Видаляє можливий шлях з оригінального імені файлу.
        var originalFileName =
            Path.GetFileName(file.FileName);

        // Отримує розширення документа.
        var extension =
            Path.GetExtension(originalFileName);

        // Генерує унікальне ім'я для фізичного збереження документа.
        var storedFileName =
            $"{Guid.NewGuid():N}{extension}";

        // Визначає каталог для збереження публічних документів зустрічей.
        var documentsDirectory = Path.Combine(
            _environment.ContentRootPath,
            "uploads",
            "PublicFile",
            "Documents");

        Directory.CreateDirectory(documentsDirectory);

        var fullFilePath = Path.Combine(
            documentsDirectory,
            storedFileName);

        try
        {
            // Зберігає фізичний файл у локальному файловому сховищі.
            await using (var fileStream = new FileStream(
                             fullFilePath,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None))
            {
                await file.CopyToAsync(fileStream);
            }

            var attachment = new MeetingAttachment
            {
                MeetingId = meetingId,
                OriginalFileName = originalFileName,
                StoredFileName = storedFileName,
                ContentType =
                    string.IsNullOrWhiteSpace(file.ContentType)
                        ? "application/octet-stream"
                        : file.ContentType,
                SizeBytes = file.Length,
                UploadedAtUtc = DateTime.UtcNow
            };

            _context.MeetingAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            var dto =
                _mapper.Map<AttachmentPublicDTO>(attachment);

            return dto with
            {
                DownloadUrl =
                    $"/api/meetings/{meetingId}" +
                    $"/attachments/{attachment.Id}/download"
            };
        }
        catch
        {
            // Видаляє фізичний файл, якщо запис метаданих у базу даних завершився помилкою.
            if (File.Exists(fullFilePath))
            {
                File.Delete(fullFilePath);
            }

            throw;
        }
    }

    /// <summary>
    /// Повертає всі публічні документи,
    /// прикріплені до вказаної зустрічі.
    /// </summary>
    /// <param name="meetingId">
    /// Ідентифікатор зустрічі.
    /// </param>
    /// <returns>
    /// Колекція публічних вкладень або
    /// <see langword="null"/>, якщо зустріч не знайдено.
    /// </returns>
    public async Task<IReadOnlyCollection<AttachmentPublicDTO>?>
        GetAllAsync(int meetingId)
    {
        var meetingExists = await _context.Meetings
            .AsNoTracking()
            .AnyAsync(meeting =>
                meeting.MeetingId == meetingId);

        if (!meetingExists)
        {
            return null;
        }

        var attachments = await _context.MeetingAttachments
            .AsNoTracking()
            .Where(attachment =>
                attachment.MeetingId == meetingId)
            .OrderByDescending(attachment =>
                attachment.UploadedAtUtc)
            .ToListAsync();

        return attachments
            .Select(attachment =>
            {
                var dto =
                    _mapper.Map<AttachmentPublicDTO>(
                        attachment);

                return dto with
                {
                    DownloadUrl =
                        $"/api/meetings/{meetingId}" +
                        $"/attachments/{attachment.Id}/download"
                };
            })
            .ToList();
    }

    /// <summary>
    /// Повертає публічний документ
    /// для завантаження клієнтом.
    /// </summary>
    /// <param name="meetingId">
    /// Ідентифікатор зустрічі.
    /// </param>
    /// <param name="attachmentId">
    /// Ідентифікатор вкладення.
    /// </param>
    /// <returns>
    /// Дані документа для завантаження або
    /// <see langword="null"/>, якщо запис чи фізичний файл не знайдено.
    /// </returns>
    public async Task<AttachmentDownloadResult?> DownloadAsync(
        int meetingId,
        int attachmentId)
    {
        var attachment = await _context.MeetingAttachments
            .AsNoTracking()
            .FirstOrDefaultAsync(attachment =>
                attachment.MeetingId == meetingId &&
                attachment.Id == attachmentId);

        if (attachment is null)
        {
            return null;
        }

        var fullFilePath = Path.Combine(
            _environment.ContentRootPath,
            "uploads",
            "PublicFile",
            "Documents",
            attachment.StoredFileName);

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
            attachment.ContentType,
            attachment.OriginalFileName);
    }

    /// <summary>
    /// Видаляє публічний документ
    /// із бази даних і файлового сховища.
    /// </summary>
    /// <param name="meetingId">
    /// Ідентифікатор зустрічі.
    /// </param>
    /// <param name="attachmentId">
    /// Ідентифікатор вкладення.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, якщо документ успішно видалено;
    /// інакше — <see langword="false"/>.
    /// </returns>
    public async Task<bool> DeleteAsync(
        int meetingId,
        int attachmentId)
    {
        var attachment = await _context.MeetingAttachments
            .FirstOrDefaultAsync(item =>
                item.Id == attachmentId &&
                item.MeetingId == meetingId);

        if (attachment is null)
        {
            return false;
        }

        var fullFilePath = Path.Combine(
            _environment.ContentRootPath,
            "uploads",
            "PublicFile",
            "Documents",
            attachment.StoredFileName);

        _context.MeetingAttachments.Remove(attachment);
        await _context.SaveChangesAsync();

        if (File.Exists(fullFilePath))
        {
            File.Delete(fullFilePath);
        }

        return true;
    }
}