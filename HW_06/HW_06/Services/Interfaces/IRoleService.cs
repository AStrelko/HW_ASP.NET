using HW_06.DTOs.IdentityDTO;
using Microsoft.AspNetCore.Identity;

namespace HW_06.Services.Interfaces;

/// <summary>
/// Визначає операції для роботи
/// з ролями користувачів.
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Повертає список усіх доступних ролей.
    /// </summary>
    /// <returns>
    /// Список назв ролей.
    /// </returns>
    Task<List<string>> GetAllRolesAsync();

    /// <summary>
    /// Повертає ролі конкретного користувача.
    /// </summary>
    /// <param name="userId">
    /// Ідентифікатор користувача ASP.NET Identity.
    /// </param>
    /// <returns>
    /// Список ролей користувача.
    /// </returns>
    Task<IList<string>> GetUserRolesAsync(int participantId);

    /// <summary>
    /// Призначає роль користувачу.
    /// </summary>
    /// <param name="dto">
    /// Ідентифікатор користувача
    /// та назва ролі.
    /// </param>
    /// <returns>
    /// Результат виконання операції Identity.
    /// </returns>
    Task<IdentityResult> AssignRoleAsync(UserRoleDTO dto);
    
}