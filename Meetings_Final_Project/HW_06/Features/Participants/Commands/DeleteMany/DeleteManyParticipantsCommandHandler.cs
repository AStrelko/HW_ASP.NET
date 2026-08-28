using HW_06.Models;
using HW_06.Models.Files;
using HW_06.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Participants.Commands.DeleteMany;

/// <summary>
/// Обробник команди масового
/// видалення учасників.
/// </summary>
public class DeleteManyParticipantsCommandHandler
    : IRequestHandler<
        DeleteManyParticipantsCommand,
        int>
{
    private const string AvatarFolder =
        "Avatars";

    private readonly DataContext _context;

    private readonly IFileStorageService
        _fileStorageService;

    private readonly UserManager<ApplicationUser>
        _userManager;

    private readonly ILogger<DeleteManyParticipantsCommandHandler>
        _logger;

    /// <summary>
    /// Ініціалізує обробник команди
    /// масового видалення учасників.
    /// </summary>
    public DeleteManyParticipantsCommandHandler(
        DataContext context,
        IFileStorageService fileStorageService,
        UserManager<ApplicationUser> userManager,
        ILogger<DeleteManyParticipantsCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(fileStorageService);
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(logger);

        _context = context;
        _fileStorageService = fileStorageService;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Видаляє учасників, їх зв'язки
    /// із зустрічами, облікові записи Identity
    /// та файли аватарів.
    /// </summary>
    public async Task<int> Handle(
        DeleteManyParticipantsCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var participantIds =
            request.Ids
                .Distinct()
                .ToList();

        var participants =
            await _context.Participants
                .Include(participant =>
                    participant.MeetingParticipants)
                .Include(participant =>
                    participant.ApplicationUser)
                .Where(participant =>
                    participantIds.Contains(
                        participant.ParticipantId))
                .ToListAsync(
                    cancellationToken);

        if (participants.Count == 0)
        {
            _logger.LogWarning(
                "Не вдалося видалити учасників. Жодного учасника за переданими ідентифікаторами не знайдено.");

            return 0;
        }

        var avatarFileNames =
            participants
                .Where(participant =>
                    !string.IsNullOrWhiteSpace(
                        participant.AvatarFileName))
                .Select(participant =>
                    participant.AvatarFileName!)
                .Distinct()
                .ToList();

        var applicationUsers =
            participants
                .Where(participant =>
                    participant.ApplicationUser is not null)
                .Select(participant =>
                    participant.ApplicationUser!)
                .ToList();

        var meetingParticipants =
            participants
                .SelectMany(participant =>
                    participant.MeetingParticipants)
                .ToList();

        _context.MeetingParticipants.RemoveRange(
            meetingParticipants);

        _context.Participants.RemoveRange(
            participants);

        await _context.SaveChangesAsync(
            cancellationToken);

        foreach (var applicationUser
                 in applicationUsers)
        {
            var identityResult =
                await _userManager.DeleteAsync(
                    applicationUser);

            if (!identityResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Не вдалося видалити обліковий запис користувача " +
                    $"{applicationUser.Email}.");
            }
        }

        foreach (var avatarFileName
                 in avatarFileNames)
        {
            await _fileStorageService.DeleteAsync(
                AvatarFolder,
                avatarFileName,
                FileAccessLevel.Public,
                cancellationToken);
        }

        _logger.LogInformation(
            "Учасників успішно видалено. Кількість: {Count}",
            participants.Count);

        return participants.Count;
    }
}