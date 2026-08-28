using HW_06.Models;
using HW_06.Models.Files;
using HW_06.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Participants.Commands.Delete;

/// <summary>
/// Обробник команди видалення учасника.
/// </summary>
public class DeleteParticipantCommandHandler
    : IRequestHandler<
        DeleteParticipantCommand,
        bool>
{
    private const string AvatarFolder =
        "Avatars";

    private readonly DataContext _context;

    private readonly IFileStorageService
        _fileStorageService;

    private readonly UserManager<ApplicationUser>
        _userManager;

    private readonly ILogger<DeleteParticipantCommandHandler>
        _logger;

    /// <summary>
    /// Ініціалізує обробник команди
    /// видалення учасника.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="fileStorageService">
    /// Сервіс файлового сховища.
    /// </param>
    /// <param name="userManager">
    /// Менеджер користувачів ASP.NET Core Identity.
    /// </param>
    /// <param name="logger">
    /// Сервіс журналювання подій
    /// видалення учасника.
    /// </param>
    public DeleteParticipantCommandHandler(
        DataContext context,
        IFileStorageService fileStorageService,
        UserManager<ApplicationUser> userManager,
        ILogger<DeleteParticipantCommandHandler> logger)
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
    /// Видаляє учасника, його зв'язки
    /// із зустрічами, обліковий запис Identity
    /// та файл аватара.
    /// </summary>
    public async Task<bool> Handle(
        DeleteParticipantCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var participant =
            await _context.Participants
                .Include(participant =>
                    participant.MeetingParticipants)
                .Include(participant =>
                    participant.ApplicationUser)
                .FirstOrDefaultAsync(
                    participant =>
                        participant.ParticipantId ==
                        request.Id,
                    cancellationToken);

        if (participant is null)
        {
            _logger.LogWarning(
                "Не вдалося видалити учасника. ParticipantId: {ParticipantId} не знайдено.",
                request.Id);

            return false;
        }

        var avatarFileName =
            participant.AvatarFileName;

        var applicationUser =
            participant.ApplicationUser;

        var firstName =
            participant.FirstName;

        var lastName =
            participant.LastName;

        _context.MeetingParticipants.RemoveRange(
            participant.MeetingParticipants);

        _context.Participants.Remove(
            participant);

        await _context.SaveChangesAsync(
            cancellationToken);

        if (applicationUser is not null)
        {
            var identityResult =
                await _userManager.DeleteAsync(
                    applicationUser);

            if (!identityResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Не вдалося видалити обліковий запис користувача.");
            }
        }

        if (!string.IsNullOrWhiteSpace(
                avatarFileName))
        {
            await _fileStorageService.DeleteAsync(
                AvatarFolder,
                avatarFileName,
                FileAccessLevel.Public,
                cancellationToken);
        }

        _logger.LogInformation(
            "Учасника успішно видалено. ParticipantId: {ParticipantId}, Name: {FirstName} {LastName}",
            request.Id,
            firstName,
            lastName);

        return true;
    }
}