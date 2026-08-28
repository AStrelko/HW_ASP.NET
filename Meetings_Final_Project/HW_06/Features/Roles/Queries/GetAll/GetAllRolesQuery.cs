using HW_06.DTOs.IdentityDTO;
using MediatR;

namespace HW_06.Features.Roles.Queries.GetAll;

/// <summary>
/// Запит для отримання
/// списку всіх доступних ролей.
/// </summary>
public record GetAllRolesQuery : IRequest<List<RoleDTO>>;