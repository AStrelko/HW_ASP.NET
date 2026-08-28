using FluentValidation;
using HW_06.Features.Common.Identity;

namespace HW_06.Features.Roles.Queries.GetUserRoles;

/// <summary>
/// Виконує перевірку запиту
/// на отримання ролей користувача.
/// </summary>
public class GetUserRolesQueryValidator
    : AbstractValidator<GetUserRolesQuery>
{
    public GetUserRolesQueryValidator()
    {
        RuleFor(query =>
                query.ParticipantId)
            .ValidParticipantId();
    }
}