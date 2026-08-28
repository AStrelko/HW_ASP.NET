using HW_06.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Roles.Commands.AssignRole;

/// <summary>
/// Обробник команди призначення
/// нової ролі учаснику.
/// </summary>
public class AssignRoleCommandHandler
    : IRequestHandler<
        AssignRoleCommand,
        IdentityResult>
{
    private readonly DataContext _context;

    private readonly RoleManager<IdentityRole>
        _roleManager;

    private readonly UserManager<ApplicationUser>
        _userManager;

    private readonly ILogger<AssignRoleCommandHandler>
        _logger;

    /// <summary>
    /// Ініціалізує обробник команди
    /// призначення ролі.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="roleManager">
    /// Менеджер ролей ASP.NET Core Identity.
    /// </param>
    /// <param name="userManager">
    /// Менеджер користувачів ASP.NET Core Identity.
    /// </param>
    /// <param name="logger">
    /// Сервіс журналювання подій
    /// призначення ролей.
    /// </param>
    public AssignRoleCommandHandler(
        DataContext context,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AssignRoleCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(roleManager);
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(logger);

        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Призначає учаснику нову роль,
    /// попередньо видаляючи всі поточні ролі.
    /// </summary>
    /// <param name="request">
    /// Команда з ідентифікатором учасника
    /// та назвою нової ролі.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування асинхронної операції.
    /// </param>
    /// <returns>
    /// Результат операції ASP.NET Core Identity.
    /// </returns>
    public async Task<IdentityResult> Handle(
        AssignRoleCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var dto =
            request.Dto;

        var participant =
            await _context.Participants
                .Include(participant =>
                    participant.ApplicationUser)
                .FirstOrDefaultAsync(
                    participant =>
                        participant.ParticipantId ==
                        dto.ParticipantId,
                    cancellationToken);

        if (participant is null)
        {
            _logger.LogWarning(
                "Не вдалося призначити роль {RoleName}. "
                + "Учасника з ParticipantId: {ParticipantId} не знайдено.",
                dto.RoleName,
                dto.ParticipantId);

            throw new KeyNotFoundException(
                $"Учасника з ідентифікатором "
                + $"{dto.ParticipantId} не знайдено.");
        }

        if (participant.ApplicationUser is null)
        {
            _logger.LogWarning(
                "Не вдалося призначити роль {RoleName}. "
                + "Обліковий запис учасника з ParticipantId: {ParticipantId} "
                + "не знайдено.",
                dto.RoleName,
                dto.ParticipantId);

            throw new KeyNotFoundException(
                $"Обліковий запис учасника "
                + $"{dto.ParticipantId} не знайдено.");
        }

        var roleExists =
            await _roleManager.RoleExistsAsync(
                dto.RoleName);

        if (!roleExists)
        {
            _logger.LogWarning(
                "Не вдалося призначити роль. "
                + "Роль {RoleName} не знайдено. "
                + "ParticipantId: {ParticipantId}",
                dto.RoleName,
                dto.ParticipantId);

            throw new KeyNotFoundException(
                $"Роль '{dto.RoleName}' не знайдено.");
        }

        var user =
            participant.ApplicationUser;

        var currentRoles =
            await _userManager.GetRolesAsync(
                user);

        if (currentRoles.Count == 1 &&
            currentRoles.Contains(
                dto.RoleName,
                StringComparer.OrdinalIgnoreCase))
        {
            _logger.LogInformation(
                "Роль {RoleName} вже призначена учаснику. "
                + "ParticipantId: {ParticipantId}",
                dto.RoleName,
                dto.ParticipantId);

            return IdentityResult.Success;
        }

        var previousRoles =
            currentRoles.Count > 0
                ? string.Join(", ", currentRoles)
                : "None";

        if (currentRoles.Count > 0)
        {
            var removeResult =
                await _userManager.RemoveFromRolesAsync(
                    user,
                    currentRoles);

            if (!removeResult.Succeeded)
            {
                _logger.LogWarning(
                    "Не вдалося видалити поточні ролі учасника. "
                    + "ParticipantId: {ParticipantId}, "
                    + "CurrentRoles: {CurrentRoles}",
                    dto.ParticipantId,
                    previousRoles);

                return removeResult;
            }
        }

        var addResult =
            await _userManager.AddToRoleAsync(
                user,
                dto.RoleName);

        if (!addResult.Succeeded)
        {
            _logger.LogWarning(
                "Не вдалося призначити роль {RoleName} учаснику. "
                + "ParticipantId: {ParticipantId}",
                dto.RoleName,
                dto.ParticipantId);

            return addResult;
        }

        _logger.LogInformation(
            "Роль учасника успішно змінено. "
            + "ParticipantId: {ParticipantId}, "
            + "PreviousRoles: {PreviousRoles}, "
            + "NewRole: {NewRole}",
            dto.ParticipantId,
            previousRoles,
            dto.RoleName);

        return addResult;
    }
}