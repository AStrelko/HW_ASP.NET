using HW_06.Services.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.PrivateAttachments.Queries.Download;

/// <summary>
/// Обробник запиту для завантаження
/// приватного файлу.
/// </summary>
public class DownloadPrivateAttachmentQueryHandler
    : IRequestHandler<
        DownloadPrivateAttachmentQuery,
        AttachmentDownloadResult?>
{
    private readonly DataContext _context;

    private readonly IWebHostEnvironment
        _environment;

    private readonly ILogger<DownloadPrivateAttachmentQueryHandler>
        _logger;

    /// <summary>
    /// Ініціалізує обробник запиту
    /// завантаження приватного файлу.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="environment">
    /// Інформація про середовище вебзастосунку.
    /// </param>
    /// <param name="logger">
    /// Сервіс журналювання подій завантаження
    /// приватних файлів.
    /// </param>
    public DownloadPrivateAttachmentQueryHandler(
        DataContext context,
        IWebHostEnvironment environment,
        ILogger<DownloadPrivateAttachmentQueryHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(logger);

        _context = context;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Завантажує приватний файл,
    /// якщо учасник є його відправником
    /// або отримувачем.
    /// </summary>
    /// <param name="request">
    /// Запит з ідентифікаторами приватного файлу
    /// та учасника.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування асинхронної операції.
    /// </param>
    /// <returns>
    /// Результат завантаження приватного файлу
    /// або <see langword="null"/>, якщо файл недоступний.
    /// </returns>
    public async Task<AttachmentDownloadResult?> Handle(
        DownloadPrivateAttachmentQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var privateFile =
            await _context.ParticipantPrivateFiles
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    file =>
                        file.Id == request.FileId &&
                        (file.SenderParticipantId ==
                            request.ParticipantId ||
                         file.RecipientParticipantId ==
                            request.ParticipantId),
                    cancellationToken);

        if (privateFile is null)
        {
            _logger.LogWarning(
                "Не вдалося завантажити приватний файл. "
                + "FileId: {FileId} не знайдено або ParticipantId: {ParticipantId} "
                + "не має доступу до нього.",
                request.FileId,
                request.ParticipantId);

            return null;
        }

        var fullFilePath =
            Path.Combine(
                _environment.ContentRootPath,
                "uploads",
                "PrivateFile",
                "Participants",
                privateFile.StoredFileName);

        if (!File.Exists(fullFilePath))
        {
            _logger.LogError(
                "Приватний файл зареєстровано в базі даних, "
                + "але фізичний файл відсутній. "
                + "FileId: {FileId}, ParticipantId: {ParticipantId}",
                privateFile.Id,
                request.ParticipantId);

            return null;
        }

        var stream =
            new FileStream(
                fullFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

        _logger.LogInformation(
            "Приватний файл успішно відкрито для завантаження. "
            + "FileId: {FileId}, ParticipantId: {ParticipantId}, "
            + "SizeBytes: {SizeBytes}",
            privateFile.Id,
            request.ParticipantId,
            privateFile.SizeBytes);

        return new AttachmentDownloadResult(
            stream,
            privateFile.ContentType,
            privateFile.OriginalFileName);
    }
}