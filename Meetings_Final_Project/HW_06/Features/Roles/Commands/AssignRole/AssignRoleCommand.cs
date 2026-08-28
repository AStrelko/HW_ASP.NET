using HW_06.DTOs.IdentityDTO;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HW_06.Features.Roles.Commands.AssignRole;

/// <summary>
/// Команда для призначення
/// нової ролі учаснику.
/// </summary>
/// <param name="Dto">
/// Ідентифікатор учасника
/// та назва нової ролі.
/// </param>
public record AssignRoleCommand(
    UserRoleDTO Dto)
    : IRequest<IdentityResult>;