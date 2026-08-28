using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Attachments.Commands.Delete;

/// <summary>
/// Обробник команди видалення
/// публічного файлу зустрічі.
/// </summary>
public class DeleteAttachmentCommandHandler
    : IRequestHandler<
        DeleteAttachmentCommand,
        bool>
{
    private readonly DataContext _context;

    private readonly IWebHostEnvironment
        _environment;

    private readonly ILogger<DeleteAttachmentCommandHandler>
        _logger;

    /// <summary>
    /// Ініціалізує обробник команди
    /// видалення публічного файлу зустрічі.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="environment">
    /// Інформація про середовище вебзастосунку.
    /// </param>
    /// <param name="logger">
    /// Сервіс журналювання подій видалення
    /// публічних файлів.
    /// </param>
    public DeleteAttachmentCommandHandler(
        DataContext context,
        IWebHostEnvironment environment,
        ILogger<DeleteAttachmentCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(logger);

        _context = context;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Видаляє публічний файл,
    /// що належить зазначеній зустрічі.
    /// </summary>
    /// <param name="request">
    /// Команда з ідентифікаторами зустрічі
    /// та публічного файлу.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування асинхронної операції.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, якщо файл успішно
    /// видалено; інакше — <see langword="false"/>.
    /// </returns>
    public async Task<bool> Handle(
        DeleteAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var attachment =
            await _context.MeetingAttachments
                .FirstOrDefaultAsync(
                    item =>
                        item.Id ==
                        request.AttachmentId &&
                        item.MeetingId ==
                        request.MeetingId,
                    cancellationToken);

        if (attachment is null)
        {
            _logger.LogWarning(
                "Не вдалося видалити публічний файл. "
                + "AttachmentId: {AttachmentId} не знайдено "
                + "для MeetingId: {MeetingId}.",
                request.AttachmentId,
                request.MeetingId);

            return false;
        }

        var fullFilePath =
            Path.Combine(
                _environment.ContentRootPath,
                "uploads",
                "PublicFile",
                "Documents",
                attachment.StoredFileName);

        _context.MeetingAttachments.Remove(
            attachment);

        await _context.SaveChangesAsync(
            cancellationToken);

        if (File.Exists(fullFilePath))
        {
            File.Delete(fullFilePath);
        }
        else
        {
            _logger.LogWarning(
                "Запис публічного файлу видалено з бази даних, "
                + "але фізичний файл не знайдено. "
                + "AttachmentId: {AttachmentId}, MeetingId: {MeetingId}",
                attachment.Id,
                attachment.MeetingId);
        }

        _logger.LogInformation(
            "Публічний файл зустрічі успішно видалено. "
            + "AttachmentId: {AttachmentId}, MeetingId: {MeetingId}",
            attachment.Id,
            attachment.MeetingId);

        return true;
    }
}