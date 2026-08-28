using HW_06.Services.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Attachments.Queries.Download;

/// <summary>
/// Обробник запиту для завантаження
/// публічного файлу зустрічі.
/// </summary>
public class DownloadAttachmentQueryHandler
    : IRequestHandler<
        DownloadAttachmentQuery,
        AttachmentDownloadResult?>
{
    private readonly DataContext _context;

    private readonly IWebHostEnvironment
        _environment;

    private readonly ILogger<DownloadAttachmentQueryHandler>
        _logger;

    /// <summary>
    /// Ініціалізує обробник запиту
    /// завантаження публічного файлу зустрічі.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="environment">
    /// Інформація про середовище вебзастосунку.
    /// </param>
    /// <param name="logger">
    /// Сервіс журналювання помилок доступу
    /// до публічних файлів.
    /// </param>
    public DownloadAttachmentQueryHandler(
        DataContext context,
        IWebHostEnvironment environment,
        ILogger<DownloadAttachmentQueryHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(logger);

        _context = context;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Повертає публічний файл
    /// для завантаження клієнтом.
    /// </summary>
    /// <param name="request">
    /// Запит з ідентифікаторами зустрічі
    /// та публічного файлу.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування асинхронної операції.
    /// </param>
    /// <returns>
    /// Результат завантаження публічного файлу
    /// або <see langword="null"/>, якщо файл не знайдено.
    /// </returns>
    public async Task<AttachmentDownloadResult?> Handle(
        DownloadAttachmentQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var attachment =
            await _context.MeetingAttachments
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    attachment =>
                        attachment.MeetingId ==
                        request.MeetingId &&
                        attachment.Id ==
                        request.AttachmentId,
                    cancellationToken);

        if (attachment is null)
        {
            return null;
        }

        var fullFilePath =
            Path.Combine(
                _environment.ContentRootPath,
                "uploads",
                "PublicFile",
                "Documents",
                attachment.StoredFileName);

        if (!File.Exists(fullFilePath))
        {
            _logger.LogError(
                "Публічний файл зареєстровано в базі даних, "
                + "але фізичний файл відсутній. "
                + "AttachmentId: {AttachmentId}, MeetingId: {MeetingId}",
                attachment.Id,
                attachment.MeetingId);

            return null;
        }

        var stream =
            new FileStream(
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
}