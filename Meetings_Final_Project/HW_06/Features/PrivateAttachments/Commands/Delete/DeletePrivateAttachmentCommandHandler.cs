using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.PrivateAttachments.Commands.Delete;

/// <summary>
/// Обробник команди видалення
/// приватного файлу його відправником.
/// </summary>
public class DeletePrivateAttachmentCommandHandler
    : IRequestHandler<
        DeletePrivateAttachmentCommand,
        bool>
{
    private readonly DataContext _context;

    private readonly IWebHostEnvironment
        _environment;

    private readonly ILogger<DeletePrivateAttachmentCommandHandler>
        _logger;

    /// <summary>
    /// Ініціалізує обробник команди
    /// видалення приватного файлу.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="environment">
    /// Інформація про середовище вебзастосунку.
    /// </param>
    /// <param name="logger">
    /// Сервіс журналювання подій видалення
    /// приватних файлів.
    /// </param>
    public DeletePrivateAttachmentCommandHandler(
        DataContext context,
        IWebHostEnvironment environment,
        ILogger<DeletePrivateAttachmentCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(logger);

        _context = context;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Видаляє приватний файл,
    /// якщо зазначений учасник є його відправником.
    /// </summary>
    /// <param name="request">
    /// Команда з ідентифікаторами приватного файлу
    /// та учасника-відправника.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування асинхронної операції.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, якщо приватний файл
    /// успішно видалено; інакше — <see langword="false"/>.
    /// </returns>
    public async Task<bool> Handle(
        DeletePrivateAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var privateFile =
            await _context.ParticipantPrivateFiles
                .FirstOrDefaultAsync(
                    file =>
                        file.Id == request.FileId &&
                        file.SenderParticipantId ==
                        request.ParticipantId,
                    cancellationToken);

        if (privateFile is null)
        {
            _logger.LogWarning(
                "Не вдалося видалити приватний файл. "
                + "FileId: {FileId} не знайдено або ParticipantId: {ParticipantId} "
                + "не є його відправником.",
                request.FileId,
                request.ParticipantId);

            return false;
        }

        var fullFilePath =
            Path.Combine(
                _environment.ContentRootPath,
                "uploads",
                "PrivateFile",
                "Participants",
                privateFile.StoredFileName);

        _context.ParticipantPrivateFiles.Remove(
            privateFile);

        await _context.SaveChangesAsync(
            cancellationToken);

        if (File.Exists(fullFilePath))
        {
            File.Delete(fullFilePath);
        }
        else
        {
            _logger.LogWarning(
                "Запис приватного файлу видалено з бази даних, "
                + "але фізичний файл не знайдено. "
                + "FileId: {FileId}, ParticipantId: {ParticipantId}",
                privateFile.Id,
                request.ParticipantId);
        }

        _logger.LogInformation(
            "Приватний файл успішно видалено відправником. "
            + "FileId: {FileId}, ParticipantId: {ParticipantId}",
            privateFile.Id,
            request.ParticipantId);

        return true;
    }
}