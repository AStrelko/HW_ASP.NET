using HW_06.DTOs.IdentityDTO;
using HW_06.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Roles.Queries.GetUserRoles;

/// <summary>
/// Обробник запиту для отримання
/// ролей користувача за ідентифікатором учасника.
/// </summary>
public class GetUserRolesQueryHandler
    : IRequestHandler<
        GetUserRolesQuery,
        IList<RoleDTO>>
{
    private readonly DataContext _context;

    private readonly UserManager<ApplicationUser>
        _userManager;

    /// <summary>
    /// Ініціалізує обробник запиту.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="userManager">
    /// Менеджер користувачів ASP.NET Core Identity.
    /// </param>
    public GetUserRolesQueryHandler(
        DataContext context,
        UserManager<ApplicationUser> userManager)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(userManager);

        _context = context;
        _userManager = userManager;
    }

    /// <summary>
    /// Повертає ролі користувача,
    /// пов'язаного із зазначеним учасником.
    /// </summary>
    public async Task<IList<RoleDTO>> Handle(
        GetUserRolesQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var participant =
            await _context.Participants
                .AsNoTracking()
                .Include(participant =>
                    participant.ApplicationUser)
                .FirstOrDefaultAsync(
                    participant =>
                        participant.ParticipantId ==
                        request.ParticipantId,
                    cancellationToken);

        if (participant is null)
        {
            throw new KeyNotFoundException(
                $"Учасника з ідентифікатором " +
                $"{request.ParticipantId} не знайдено.");
        }

        if (participant.ApplicationUser is null)
        {
            throw new KeyNotFoundException(
                $"Обліковий запис учасника " +
                $"{request.ParticipantId} не знайдено.");
        }

        var roles =
            await _userManager.GetRolesAsync(
                participant.ApplicationUser);

        return roles
            .Select(roleName =>
                new RoleDTO
                {
                    RoleName =
                        roleName
                })
            .ToList();
    }
}