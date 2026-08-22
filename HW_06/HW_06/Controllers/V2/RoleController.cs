using HW_06.DTOs.IdentityDTO;
using HW_06.Filters;
using HW_06.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HW_06.Controllers;

/// <summary>
/// Керує ролями користувачів.
/// Доступ до операцій має лише адміністратор.
/// </summary>
[ApiController]
[Route("api/roles")]
[Authorize(Roles = "Admin")]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    /// <summary>
    /// Ініціалізує контролер
    /// для роботи з ролями користувачів.
    /// </summary>
    /// <param name="roleService">
    /// Сервіс для роботи з ролями.
    /// </param>
    public RoleController(IRoleService roleService)
    {
        ArgumentNullException.ThrowIfNull(roleService);

        _roleService = roleService;
    }

    /// <summary>
    /// Повертає список усіх доступних ролей.
    /// </summary>
    /// <returns>
    /// Список назв ролей.
    /// </returns>
    /// <response code="200">
    /// Список ролей успішно отримано.
    /// </response>
    /// <response code="401">
    /// Користувач не автентифікований.
    /// </response>
    /// <response code="403">
    /// Користувач не має ролі Admin.
    /// </response>
    [HttpGet]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<string>>> GetAll()
    {
        var roles = await _roleService.GetAllRolesAsync();

        return Ok(roles);
    }

    /// <summary>
    /// Повертає ролі користувача,
    /// пов'язаного з указаним учасником.
    /// </summary>
    /// <param name="participantId">
    /// Ідентифікатор учасника.
    /// </param>
    /// <returns>
    /// Список ролей користувача,
    /// пов'язаного з учасником.
    /// </returns>
    /// <response code="200">
    /// Ролі користувача успішно отримано.
    /// </response>
    /// <response code="401">
    /// Користувач не автентифікований.
    /// </response>
    /// <response code="403">
    /// Користувач не має ролі Admin.
    /// </response>
    /// <response code="404">
    /// Учасника або пов'язаний обліковий запис не знайдено.
    /// </response>
    [HttpGet("participants/{participantId:int}")]
    [ProducesResponseType(typeof(IList<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IList<string>>> GetUserRoles(int participantId)
    {
        var roles = await _roleService.GetUserRolesAsync(participantId);

        return Ok(roles);
    }

    /// <summary>
    /// Змінює роль користувача,
    /// пов'язаного з указаним учасником.
    /// Поточна роль буде замінена новою.
    /// </summary>
    /// <param name="dto">
    /// Ідентифікатор учасника
    /// та назва нової ролі.
    /// </param>
    /// <response code="200">
    /// Роль користувача успішно змінено.
    /// </response>
    /// <response code="400">
    /// Дані не пройшли перевірку
    /// або роль не вдалося змінити.
    /// </response>
    /// <response code="401">
    /// Користувач не автентифікований.
    /// </response>
    /// <response code="403">
    /// Користувач не має ролі Admin.
    /// </response>
    /// <response code="404">
    /// Учасника, обліковий запис або роль не знайдено.
    /// </response>
    [HttpPut("assign")]
    [ServiceFilter(typeof(ValidationFilter<UserRoleDTO>))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRole(UserRoleDTO dto)
    {
        var result = await _roleService.AssignRoleAsync(dto);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok(new
        {
            Message = "Роль користувача успішно змінено."
        });
    }
}