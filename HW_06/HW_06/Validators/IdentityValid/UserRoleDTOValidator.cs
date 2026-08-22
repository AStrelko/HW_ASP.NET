using FluentValidation;
using HW_06.DTOs.IdentityDTO;

namespace HW_06.Validators.IdentityValid;

/// <summary>
/// Виконує перевірку даних
/// для зміни ролі учасника.
/// </summary>
public class UserRoleDTOValidator
    : AbstractValidator<UserRoleDTO>
{
    public UserRoleDTOValidator()
    {
        RuleFor(x => x.ParticipantId)
            .ValidParticipantId();

        RuleFor(x => x.RoleName)
            .ValidRoleName();
    }
}