using HW_06.Models.Files;
using HW_06.Services.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Participants.Commands.ResetAvatar;

/// <summary>
/// Обробник команди скидання
/// аватара учасника.
/// </summary>
public class ResetParticipantAvatarCommandHandler
    : IRequestHandler<
        ResetParticipantAvatarCommand,
        bool>
{
    private const string AvatarFolder =
        "Avatars";

    private readonly DataContext _context;

    private readonly IFileStorageService
        _fileStorageService;

    private readonly ILogger<ResetParticipantAvatarCommandHandler>
        _logger;

    /// <summary>
    /// Ініціалізує обробник команди
    /// скидання аватара учасника.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="fileStorageService">
    /// Сервіс файлового сховища.
    /// </param>
    /// <param name="logger">
    /// Сервіс журналювання подій
    /// скидання аватара учасника.
    /// </param>
    public ResetParticipantAvatarCommandHandler(
        DataContext context,
        IFileStorageService fileStorageService,
        ILogger<ResetParticipantAvatarCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(fileStorageService);
        ArgumentNullException.ThrowIfNull(logger);

        _context = context;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Видаляє власний аватар учасника
    /// та повертає використання стандартного аватара.
    /// </summary>
    public async Task<bool> Handle(
        ResetParticipantAvatarCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var participant =
            await _context.Participants
                .FirstOrDefaultAsync(
                    participant =>
                        participant.ParticipantId ==
                        request.ParticipantId,
                    cancellationToken);

        if (participant is null)
        {
            _logger.LogWarning(
                "Не вдалося скинути аватар. ParticipantId: {ParticipantId} не знайдено.",
                request.ParticipantId);

            return false;
        }

        if (!string.IsNullOrWhiteSpace(
                participant.AvatarFileName))
        {
            await _fileStorageService.DeleteAsync(
                AvatarFolder,
                participant.AvatarFileName,
                FileAccessLevel.Public,
                cancellationToken);
        }

        participant.AvatarFileName =
            null;

        await _context.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "Аватар учасника успішно скинуто. ParticipantId: {ParticipantId}, Name: {FirstName} {LastName}",
            participant.ParticipantId,
            participant.FirstName,
            participant.LastName);

        return true;
    }
}