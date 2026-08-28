using MediatR;
using HW_06.DTOs.IdentityDTO;

namespace HW_06.Features.Roles.Queries.GetUserRoles;

/// <summary>
/// Запит для отримання ролей користувача,
/// пов'язаного із зазначеним учасником.
/// </summary>
/// <param name="ParticipantId">
/// Ідентифікатор учасника.
/// </param>
public record GetUserRolesQuery(
    int ParticipantId)
    : IRequest<IList<RoleDTO>>;