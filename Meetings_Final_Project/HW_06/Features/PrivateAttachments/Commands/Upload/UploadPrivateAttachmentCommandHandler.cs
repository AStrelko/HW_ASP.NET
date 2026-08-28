using AutoMapper;
using HW_06.DTOs.Files;
using HW_06.Features.Common.Files;
using HW_06.Models;
using HW_06.Storage.Configurations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HW_06.Features.PrivateAttachments.Commands.Upload;

/// <summary>
/// Обробник команди надсилання
/// приватного файлу між учасниками.
/// </summary>
public class UploadPrivateAttachmentCommandHandler
    : IRequestHandler<
        UploadPrivateAttachmentCommand,
        AttachmentPrivateDTO?>
{
    private readonly DataContext _context;

    private readonly IMapper _mapper;

    private readonly IWebHostEnvironment
        _environment;

    private readonly FileStorageOptions
        _fileStorageOptions;

    private readonly ILogger<UploadPrivateAttachmentCommandHandler>
        _logger;

    /// <summary>
    /// Ініціалізує обробник команди
    /// надсилання приватного файлу.
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
    /// Сервіс журналювання подій надсилання
    /// приватних файлів.
    /// </param>
    public UploadPrivateAttachmentCommandHandler(
        DataContext context,
        IMapper mapper,
        IWebHostEnvironment environment,
        IOptions<FileStorageOptions> fileStorageOptions,
        ILogger<UploadPrivateAttachmentCommandHandler> logger)
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
    /// Надсилає приватний файл
    /// від одного учасника іншому.
    /// </summary>
    /// <param name="request">
    /// Команда з даними приватного файлу,
    /// відправника та отримувача.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування асинхронної операції.
    /// </param>
    /// <returns>
    /// Дані надісланого приватного файлу.
    /// </returns>
    public async Task<AttachmentPrivateDTO?> Handle(
        UploadPrivateAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var sender =
            await _context.Participants
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    participant =>
                        participant.ParticipantId ==
                        request.SenderParticipantId,
                    cancellationToken);

        if (sender is null)
        {
            _logger.LogWarning(
                "Не вдалося надіслати приватний файл. "
                + "Учасника-відправника з ParticipantId: {SenderParticipantId} "
                + "не знайдено.",
                request.SenderParticipantId);

            throw new KeyNotFoundException(
                $"Учасника-відправника з ідентифікатором "
                + $"{request.SenderParticipantId} не знайдено.");
        }

        var recipient =
            await _context.Participants
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    participant =>
                        participant.ParticipantId ==
                        request.RecipientParticipantId,
                    cancellationToken);

        if (recipient is null)
        {
            _logger.LogWarning(
                "Не вдалося надіслати приватний файл. "
                + "Учасника-отримувача з ParticipantId: {RecipientParticipantId} "
                + "не знайдено.",
                request.RecipientParticipantId);

            throw new KeyNotFoundException(
                $"Учасника-отримувача з ідентифікатором "
                + $"{request.RecipientParticipantId} не знайдено.");
        }

        var validationError =
            await PrivateDocumentValidator.ValidateAsync(
                request.File,
                _fileStorageOptions
                    .MaxPrivateDocumentSizeBytes);

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

        var privateFilesDirectory =
            Path.Combine(
                _environment.ContentRootPath,
                "uploads",
                "PrivateFile",
                "Participants");

        Directory.CreateDirectory(
            privateFilesDirectory);

        var fullFilePath =
            Path.Combine(
                privateFilesDirectory,
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

            var privateFile =
                new ParticipantPrivateFile
                {
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
                        DateTime.UtcNow,

                    SenderParticipantId =
                        request.SenderParticipantId,

                    RecipientParticipantId =
                        request.RecipientParticipantId
                };

            _context.ParticipantPrivateFiles.Add(
                privateFile);

            await _context.SaveChangesAsync(
                cancellationToken);

            var savedPrivateFile =
                await _context.ParticipantPrivateFiles
                    .AsNoTracking()
                    .Include(item =>
                        item.SenderParticipant)
                    .Include(item =>
                        item.RecipientParticipant)
                    .FirstAsync(
                        item =>
                            item.Id ==
                            privateFile.Id,
                        cancellationToken);

            var dto =
                _mapper.Map<AttachmentPrivateDTO>(
                    savedPrivateFile);

            _logger.LogInformation(
                "Приватний файл успішно надіслано. "
                + "FileId: {FileId}, "
                + "SenderParticipantId: {SenderParticipantId}, "
                + "RecipientParticipantId: {RecipientParticipantId}, "
                + "SizeBytes: {SizeBytes}",
                savedPrivateFile.Id,
                request.SenderParticipantId,
                request.RecipientParticipantId,
                savedPrivateFile.SizeBytes);

            return dto with
            {
                DownloadUrl =
                    $"/api/participants/"
                    + $"{request.RecipientParticipantId}"
                    + $"/private-files/{savedPrivateFile.Id}/download"
            };
        }
        catch
        {
            if (File.Exists(fullFilePath))
            {
                File.Delete(fullFilePath);

                _logger.LogWarning(
                    "Фізичний приватний файл видалено під час відкату "
                    + "невдалої операції надсилання. "
                    + "SenderParticipantId: {SenderParticipantId}, "
                    + "RecipientParticipantId: {RecipientParticipantId}",
                    request.SenderParticipantId,
                    request.RecipientParticipantId);
            }

            throw;
        }
    }
}