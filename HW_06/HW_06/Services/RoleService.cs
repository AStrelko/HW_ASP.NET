using HW_06.DTOs.IdentityDTO;
using HW_06.Models;
using HW_06.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Services;

/// <summary>
/// Сервіс для роботи
/// з ролями користувачів.
/// </summary>
public class RoleService : IRoleService
{
    private readonly DataContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Ініціалізує сервіс роботи з ролями.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="roleManager">
    /// Менеджер ролей ASP.NET Identity.
    /// </param>
    /// <param name="userManager">
    /// Менеджер користувачів ASP.NET Identity.
    /// </param>
    public RoleService(
        DataContext context,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(roleManager);
        ArgumentNullException.ThrowIfNull(userManager);

        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    /// <summary>
    /// Повертає список усіх доступних ролей.
    /// </summary>
    public async Task<List<string>> GetAllRolesAsync()
    {
        return await _roleManager.Roles
            .Where(role =>
                role.Name != null)
            .Select(role =>
                role.Name!)
            .OrderBy(roleName =>
                roleName)
            .ToListAsync();
    }

    /// <summary>
    /// Повертає ролі користувача,
    /// пов'язаного з указаним учасником.
    /// </summary>
    /// <param name="participantId">
    /// Ідентифікатор учасника.
    /// </param>
    public async Task<IList<string>> GetUserRolesAsync(
        int participantId)
    {
        var participant =
            await _context.Participants
                .AsNoTracking()
                .Include(participant =>
                    participant.ApplicationUser)
                .FirstOrDefaultAsync(participant =>
                    participant.ParticipantId ==
                    participantId);

        if (participant is null)
        {
            throw new KeyNotFoundException(
                $"Учасника з ідентифікатором " +
                $"{participantId} не знайдено.");
        }

        if (participant.ApplicationUser is null)
        {
            throw new KeyNotFoundException(
                $"Обліковий запис учасника " +
                $"{participantId} не знайдено.");
        }

        return await _userManager.GetRolesAsync(
            participant.ApplicationUser);
    }

    /// <summary>
    /// Призначає учаснику нову роль,
    /// попередньо видаляючи всі поточні ролі.
    /// </summary>
    /// <param name="dto">
    /// Ідентифікатор учасника
    /// та назва нової ролі.
    /// </param>
    public async Task<IdentityResult> AssignRoleAsync(
        UserRoleDTO dto)
    {
        var participant =
            await _context.Participants
                .Include(participant =>
                    participant.ApplicationUser)
                .FirstOrDefaultAsync(participant =>
                    participant.ParticipantId ==
                    dto.ParticipantId);

        if (participant is null)
        {
            throw new KeyNotFoundException(
                $"Учасника з ідентифікатором " +
                $"{dto.ParticipantId} не знайдено.");
        }

        if (participant.ApplicationUser is null)
        {
            throw new KeyNotFoundException(
                $"Обліковий запис учасника " +
                $"{dto.ParticipantId} не знайдено.");
        }

        var roleExists =
            await _roleManager.RoleExistsAsync(
                dto.RoleName);

        if (!roleExists)
        {
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
            return IdentityResult.Success;
        }

        if (currentRoles.Count > 0)
        {
            var removeResult =
                await _userManager.RemoveFromRolesAsync(
                    user,
                    currentRoles);

            if (!removeResult.Succeeded)
            {
                return removeResult;
            }
        }

        return await _userManager.AddToRoleAsync(
            user,
            dto.RoleName);
    }
}