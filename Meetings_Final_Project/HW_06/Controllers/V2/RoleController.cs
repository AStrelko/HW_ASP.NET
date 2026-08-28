using HW_06.Common.Constants;
using HW_06.DTOs.IdentityDTO;
using HW_06.Features.Roles.Commands.AssignRole;
using HW_06.Features.Roles.Queries.GetAll;
using HW_06.Features.Roles.Queries.GetUserRoles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HW_06.Controllers;

/// <summary>
/// Керує ролями користувачів.
/// Доступ до операцій має лише адміністратор.
/// </summary>
[ApiController]
[Route("api/roles")]
[Authorize(Roles = ApplicationRoles.Admin)]
public class RoleController : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>
    /// Ініціалізує контролер
    /// для роботи з ролями користувачів.
    /// </summary>
    /// <param name="sender">
    /// Сервіс MediatR для надсилання команд і запитів.
    /// </param>
    public RoleController( ISender sender)
    {
        ArgumentNullException.ThrowIfNull(sender);

        _sender = sender;
    }

    /// <summary>
    /// Повертає список усіх доступних ролей.
    /// </summary>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
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
    [ProducesResponseType(typeof(List<RoleDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<RoleDTO>>> GetAll(CancellationToken cancellationToken)
    {
        var roles = await _sender.Send( new GetAllRolesQuery(), cancellationToken);
        return Ok(roles);
    }

    /// <summary>
    /// Повертає ролі користувача,
    /// пов'язаного з указаним учасником.
    /// </summary>
    /// <param name="participantId">
    /// Ідентифікатор учасника.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
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
    /// Учасника або пов'язаний
    /// обліковий запис не знайдено.
    /// </response>
    [HttpGet("participants/{participantId:int}")]
    [ProducesResponseType(typeof(IList<RoleDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IList<RoleDTO>>> GetUserRoles(
        int participantId,
        CancellationToken cancellationToken)
    {
        var roles = await _sender.Send(
            new GetUserRolesQuery(participantId),
            cancellationToken);

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
    /// <param name="cancellationToken">
    /// Токен скасування операції.
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
    /// Учасника, обліковий запис
    /// або роль не знайдено.
    /// </response>
    [HttpPut("assign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRole(UserRoleDTO dto, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AssignRoleCommand(dto), cancellationToken);

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