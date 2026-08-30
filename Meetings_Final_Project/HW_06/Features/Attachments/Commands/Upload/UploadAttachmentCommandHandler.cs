using AutoMapper;
using HW_06.DTOs.Files;
using HW_06.Features.Common.Files;
using HW_06.Models;
using HW_06.Storage.Configurations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HW_06.Features.Attachments.Commands.Upload;

/// <summary>
/// Обробник команди завантаження
/// публічного файлу до зустрічі.
/// </summary>
public class UploadAttachmentCommandHandler
    : IRequestHandler<
        UploadAttachmentCommand,
        AttachmentPublicDTO?>
{
    private readonly DataContext _context;

    private readonly IMapper _mapper;

    private readonly IWebHostEnvironment _environment;

    private readonly FileStorageOptions _fileStorageOptions;

    private readonly ILogger<UploadAttachmentCommandHandler>
        _logger;

    /// <summary>
    /// Ініціалізує обробник команди
    /// завантаження публічного файлу.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="mapper">
    /// Сервіс перетворення моделей у DTO.
    /// </param>
    /// <param name="environment">
    /// Інформація про середовище вебзастосунку.
    /// </param>
    /// <param name="fileStorageOptions">
    /// Налаштування файлового сховища.
    /// </param>
    /// <param name="logger">
    /// Сервіс журналювання подій завантаження файлів.
    /// </param>
    public UploadAttachmentCommandHandler(
        DataContext context,
        IMapper mapper,
        IWebHostEnvironment environment,
        IOptions<FileStorageOptions> fileStorageOptions,
        ILogger<UploadAttachmentCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(fileStorageOptions);
        ArgumentNullException.ThrowIfNull(logger);

        _context = context;
        _mapper = mapper;
        _environment = environment;
        _fileStorageOptions = fileStorageOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Зберігає публічний файл
    /// та прикріплює його до зустрічі.
    /// Помилка формування DTO після успішного
    /// збереження запису в БД не спричиняє видалення файлу.
    /// </summary>
    /// <param name="request">
    /// Команда з ідентифікатором зустрічі та файлом.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування асинхронної операції.
    /// </param>
    /// <returns>
    /// Дані завантаженого файлу або
    /// <see langword="null"/>, якщо зустріч не знайдено.
    /// </returns>
    public async Task<AttachmentPublicDTO?> Handle(
        UploadAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var meetingExists =
            await _context.Meetings
                .AnyAsync(
                    meeting =>
                        meeting.MeetingId == request.MeetingId,
                    cancellationToken);

        if (!meetingExists)
        {
            _logger.LogWarning(
                "Не вдалося завантажити публічний файл. "
                + "Зустріч з MeetingId: {MeetingId} не знайдено.",
                request.MeetingId);

            return null;
        }

        var validationError =
            await PublicDocumentValidator.ValidateAsync(
                request.File,
                _fileStorageOptions.MaxPublicDocumentSizeBytes);

        if (validationError is not null)
        {
            throw new FluentValidation.ValidationException(
                validationError);
        }

        var originalFileName =
            Path.GetFileName(
                request.File.FileName);

        var extension =
            Path.GetExtension(
                originalFileName);

        var storedFileName =
            $"{Guid.NewGuid():N}{extension}";

        var documentsDirectory =
            Path.Combine(
                _environment.ContentRootPath,
                "uploads",
                "PublicFile",
                "Documents");

        Directory.CreateDirectory(
            documentsDirectory);

        var fullFilePath =
            Path.Combine(
                documentsDirectory,
                storedFileName);

        var attachment =
            new MeetingAttachment
            {
                MeetingId =
                    request.MeetingId,

                OriginalFileName =
                    originalFileName,

                StoredFileName =
                    storedFileName,

                ContentType =
                    string.IsNullOrWhiteSpace(
                        request.File.ContentType)
                        ? "application/octet-stream"
                        : request.File.ContentType,

                SizeBytes =
                    request.File.Length,

                UploadedAtUtc =
                    DateTime.UtcNow
            };

        // Видаляємо під час відкату лише файл,
        // створений поточною операцією.
        var fileCreated = false;

        try
        {
            await using (var fileStream =
                         new FileStream(
                             fullFilePath,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None,
                             bufferSize: 81920,
                             useAsync: true))
            {
                fileCreated = true;

                await request.File.CopyToAsync(
                    fileStream,
                    cancellationToken);
            }

            _context.MeetingAttachments.Add(
                attachment);

            await _context.SaveChangesAsync(
                cancellationToken);
        }
        catch
        {
            if (fileCreated)
            {
                try
                {
                    // File.Delete не кидає виняток,
                    // якщо файл уже відсутній.
                    File.Delete(
                        fullFilePath);

                    _logger.LogWarning(
                        "Виконано очищення публічного файлу "
                        + "після невдалої операції завантаження. "
                        + "MeetingId: {MeetingId}, "
                        + "StoredFileName: {StoredFileName}",
                        request.MeetingId,
                        storedFileName);
                }
                catch (Exception cleanupException)
                {
                    // Помилка очищення не повинна
                    // підміняти початковий виняток.
                    _logger.LogError(
                        cleanupException,
                        "Не вдалося видалити публічний файл "
                        + "під час очищення після помилки. "
                        + "MeetingId: {MeetingId}, "
                        + "StoredFileName: {StoredFileName}",
                        request.MeetingId,
                        storedFileName);
                }
            }

            throw;
        }

        // Запис успішно збережено в БД.
        // Подальші помилки не запускають видалення файлу.

        _logger.LogInformation(
            "Публічний файл успішно завантажено до зустрічі. "
            + "AttachmentId: {AttachmentId}, "
            + "MeetingId: {MeetingId}, "
            + "SizeBytes: {SizeBytes}",
            attachment.Id,
            attachment.MeetingId,
            attachment.SizeBytes);

        var dto =
            _mapper.Map<AttachmentPublicDTO>(
                attachment);

        return dto with
        {
            DownloadUrl =
                $"/api/meetings/{request.MeetingId}"
                + $"/attachments/{attachment.Id}/download"
        };
    }
}