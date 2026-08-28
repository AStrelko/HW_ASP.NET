using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.PrivateAttachments.Commands.DeleteByAdmin;

/// <summary>
/// Обробник команди видалення
/// приватного файлу адміністратором.
/// </summary>
public class DeletePrivateAttachmentByAdminCommandHandler
    : IRequestHandler<
        DeletePrivateAttachmentByAdminCommand,
        bool>
{
    private readonly DataContext _context;

    private readonly IWebHostEnvironment
        _environment;

    private readonly ILogger<DeletePrivateAttachmentByAdminCommandHandler>
        _logger;

    /// <summary>
    /// Ініціалізує обробник команди
    /// видалення приватного файлу адміністратором.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="environment">
    /// Інформація про середовище вебзастосунку.
    /// </param>
    /// <param name="logger">
    /// Сервіс журналювання подій видалення
    /// приватних файлів адміністратором.
    /// </param>
    public DeletePrivateAttachmentByAdminCommandHandler(
        DataContext context,
        IWebHostEnvironment environment,
        ILogger<DeletePrivateAttachmentByAdminCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(logger);

        _context = context;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Видаляє приватний файл
    /// незалежно від його відправника або отримувача.
    /// </summary>
    /// <param name="request">
    /// Команда з ідентифікатором приватного файлу.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування асинхронної операції.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, якщо приватний файл
    /// успішно видалено; інакше — <see langword="false"/>.
    /// </returns>
    public async Task<bool> Handle(
        DeletePrivateAttachmentByAdminCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var privateFile =
            await _context.ParticipantPrivateFiles
                .FirstOrDefaultAsync(
                    file =>
                        file.Id == request.FileId,
                    cancellationToken);

        if (privateFile is null)
        {
            _logger.LogWarning(
                "Не вдалося видалити приватний файл адміністратором. "
                + "FileId: {FileId} не знайдено.",
                request.FileId);

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
                "Запис приватного файлу видалено з бази даних адміністратором, "
                + "але фізичний файл не знайдено. FileId: {FileId}",
                privateFile.Id);
        }

        _logger.LogInformation(
            "Приватний файл успішно видалено адміністратором. "
            + "FileId: {FileId}",
            privateFile.Id);

        return true;
    }
}