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

    private readonly IWebHostEnvironment
        _environment;

    private readonly FileStorageOptions
        _fileStorageOptions;

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
    /// Сервіс журналювання подій завантаження
    /// публічних файлів.
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
    /// Завантажує публічний файл
    /// та прикріплює його до зустрічі.
    /// </summary>
    /// <param name="request">
    /// Команда з ідентифікатором зустрічі
    /// та публічним файлом.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування асинхронної операції.
    /// </param>
    /// <returns>
    /// Дані завантаженого публічного файлу
    /// або <see langword="null"/>, якщо зустріч не знайдено.
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
                        meeting.MeetingId ==
                        request.MeetingId,
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
                _fileStorageOptions
                    .MaxPublicDocumentSizeBytes);

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

        try
        {
            await using (var fileStream =
                         new FileStream(
                             fullFilePath,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None))
            {
                await request.File.CopyToAsync(
                    fileStream,
                    cancellationToken);
            }

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

            _context.MeetingAttachments.Add(
                attachment);

            await _context.SaveChangesAsync(
                cancellationToken);

            var dto =
                _mapper.Map<AttachmentPublicDTO>(
                    attachment);

            _logger.LogInformation(
                "Публічний файл успішно завантажено до зустрічі. "
                + "AttachmentId: {AttachmentId}, "
                + "MeetingId: {MeetingId}, "
                + "SizeBytes: {SizeBytes}",
                attachment.Id,
                attachment.MeetingId,
                attachment.SizeBytes);

            return dto with
            {
                DownloadUrl =
                    $"/api/meetings/{request.MeetingId}"
                    + $"/attachments/{attachment.Id}/download"
            };
        }
        catch
        {
            if (File.Exists(fullFilePath))
            {
                File.Delete(fullFilePath);

                _logger.LogWarning(
                    "Фізичний публічний файл видалено під час відкату "
                    + "невдалої операції завантаження. "
                    + "MeetingId: {MeetingId}",
                    request.MeetingId);
            }

            throw;
        }
    }
}