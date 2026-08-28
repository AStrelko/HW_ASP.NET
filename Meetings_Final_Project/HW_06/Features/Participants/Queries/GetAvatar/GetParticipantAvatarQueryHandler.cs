using HW_06.Models.Files;
using HW_06.Services.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Participants.Queries.GetAvatar;

/// <summary>
/// Обробник запиту для отримання
/// аватара учасника.
/// </summary>
public class GetParticipantAvatarQueryHandler
    : IRequestHandler<
        GetParticipantAvatarQuery,
        FileDownloadResult?>
{
    private const string AvatarFolder =
        "Avatars";

    private const string DefaultAvatarFileName =
        "default";

    private readonly DataContext _context;
    private readonly IFileStorageService _fileStorageService;

    /// <summary>
    /// Ініціалізує обробник запиту.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="fileStorageService">
    /// Сервіс файлового сховища.
    /// </param>
    public GetParticipantAvatarQueryHandler(
        DataContext context,
        IFileStorageService fileStorageService)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(fileStorageService);

        _context = context;
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// Повертає власний аватар учасника
    /// або стандартний аватар.
    /// </summary>
    public async Task<FileDownloadResult?> Handle(
        GetParticipantAvatarQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var participant =
            await _context.Participants
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    participant =>
                        participant.ParticipantId ==
                        request.ParticipantId,
                    cancellationToken);

        if (participant is null)
        {
            return null;
        }

        var avatarFileName =
            string.IsNullOrWhiteSpace(
                participant.AvatarFileName)
                ? DefaultAvatarFileName
                : participant.AvatarFileName;

        var file =
            _fileStorageService.OpenRead(
                AvatarFolder,
                avatarFileName,
                FileAccessLevel.Public);

        if (file is not null)
        {
            return file;
        }

        if (avatarFileName !=
            DefaultAvatarFileName)
        {
            return _fileStorageService.OpenRead(
                AvatarFolder,
                DefaultAvatarFileName,
                FileAccessLevel.Public);
        }

        return null;
    }
}