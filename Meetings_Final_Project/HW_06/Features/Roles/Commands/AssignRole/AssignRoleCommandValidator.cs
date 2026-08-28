using FluentValidation;
using HW_06.Features.Common.Identity;

namespace HW_06.Features.Roles.Commands.AssignRole;

/// <summary>
/// Виконує перевірку команди
/// призначення ролі учаснику.
/// </summary>
public class AssignRoleCommandValidator
    : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleCommandValidator()
    {
        RuleFor(command =>
                command.Dto.ParticipantId)
            .ValidParticipantId();

        RuleFor(command =>
                command.Dto.RoleName)
            .ValidRoleName();
    }
}