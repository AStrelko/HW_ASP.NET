using HW_06.DTOs.ParticipantsDTO;
using HW_06.Models.Files;
using HW_06.Services.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Participants.Commands.UploadAvatar;

/// <summary>
/// Обробник команди додавання
/// або заміни аватара учасника.
/// </summary>
public class UploadParticipantAvatarCommandHandler
    : IRequestHandler<
        UploadParticipantAvatarCommand,
        ParticipantAvatarDTO?>
{
    /// <summary>
    /// Каталог для зберігання
    /// файлів аватарів.
    /// </summary>
    private const string AvatarFolder =
        "Avatars";

    private readonly DataContext _context;

    private readonly IFileStorageService
        _fileStorageService;

    private readonly ILogger<UploadParticipantAvatarCommandHandler>
        _logger;

    /// <summary>
    /// Ініціалізує обробник команди
    /// завантаження аватара.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="fileStorageService">
    /// Сервіс файлового сховища.
    /// </param>
    /// <param name="logger">
    /// Сервіс журналювання подій
    /// завантаження аватара.
    /// </param>
    public UploadParticipantAvatarCommandHandler(
        DataContext context,
        IFileStorageService fileStorageService,
        ILogger<UploadParticipantAvatarCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(
            fileStorageService);
        ArgumentNullException.ThrowIfNull(logger);

        _context = context;
        _fileStorageService =
            fileStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Додає або замінює
    /// аватар зазначеного учасника.
    /// </summary>
    public async Task<ParticipantAvatarDTO?> Handle(
        UploadParticipantAvatarCommand request,
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
                "Не вдалося завантажити аватар. ParticipantId: {ParticipantId} не знайдено.",
                request.ParticipantId);

            return null;
        }

        string savedFileName;

        if (!string.IsNullOrWhiteSpace(
                participant.AvatarFileName))
        {
            savedFileName =
                await _fileStorageService.ReplaceAsync(
                    request.File,
                    AvatarFolder,
                    participant.AvatarFileName,
                    FileAccessLevel.Public,
                    cancellationToken);
        }
        else
        {
            savedFileName =
                await _fileStorageService.SaveAsync(
                    request.File,
                    AvatarFolder,
                    FileAccessLevel.Public,
                    cancellationToken);
        }

        participant.AvatarFileName =
            savedFileName;

        await _context.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "Аватар учасника успішно завантажено. ParticipantId: {ParticipantId}, Name: {FirstName} {LastName}, FileName: {FileName}",
            participant.ParticipantId,
            participant.FirstName,
            participant.LastName,
            savedFileName);

        return new ParticipantAvatarDTO
        {
            ParticipantId =
                participant.ParticipantId,

            FirstName =
                participant.FirstName,

            LastName =
                participant.LastName,

            AvatarFileName =
                participant.AvatarFileName,

            AvatarUrl =
                null
        };
    }
}