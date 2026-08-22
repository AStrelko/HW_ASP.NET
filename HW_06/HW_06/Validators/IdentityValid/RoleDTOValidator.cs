using FluentValidation;
using HW_06.DTOs.IdentityDTO;

namespace HW_06.Validators.IdentityValid;

/// <summary>
/// Виконує перевірку даних ролі.
/// </summary>
public class RoleDTOValidator
    : AbstractValidator<RoleDTO>
{
    public RoleDTOValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty()
            .WithMessage(
                "Назва ролі є обов’язковою.")
            .MaximumLength(50)
            .WithMessage(
                "Назва ролі не може перевищувати 50 символів.");
    }
}