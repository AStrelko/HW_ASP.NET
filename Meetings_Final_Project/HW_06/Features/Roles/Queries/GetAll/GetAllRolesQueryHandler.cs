using HW_06.DTOs.IdentityDTO;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Roles.Queries.GetAll;

/// <summary>
/// Обробник запиту для отримання
/// списку всіх доступних ролей.
/// </summary>
public class GetAllRolesQueryHandler
    : IRequestHandler<
        GetAllRolesQuery,
        List<RoleDTO>>
{
    private readonly RoleManager<IdentityRole>
        _roleManager;

    /// <summary>
    /// Ініціалізує обробник запиту.
    /// </summary>
    /// <param name="roleManager">
    /// Менеджер ролей ASP.NET Core Identity.
    /// </param>
    public GetAllRolesQueryHandler(
        RoleManager<IdentityRole> roleManager)
    {
        ArgumentNullException.ThrowIfNull(
            roleManager);

        _roleManager = roleManager;
    }

    /// <summary>
    /// Повертає список
    /// усіх доступних ролей.
    /// </summary>
    public async Task<List<RoleDTO>> Handle(
        GetAllRolesQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await _roleManager.Roles
            .Where(role =>
                role.Name != null)
            .OrderBy(role =>
                role.Name)
            .Select(role =>
                new RoleDTO
                {
                    RoleName = role.Name!
                })
            .ToListAsync(
                cancellationToken);
    }
}