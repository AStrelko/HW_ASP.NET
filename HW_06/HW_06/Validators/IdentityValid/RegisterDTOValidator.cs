using FluentValidation;
using HW_06.DTOs.IdentityDTO;

namespace HW_06.Validators.IdentityValid;

/// <summary>
/// Виконує перевірку даних
/// для реєстрації нового користувача.
/// </summary>
public class RegisterDTOValidator
    : AbstractValidator<RegisterDTO>
{
    public RegisterDTOValidator()
    {
        RuleFor(x => x.Email)
            .ValidEmail();

        RuleFor(x => x.Password)
            .ValidPassword();

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage(
                "Підтвердження пароля є обов’язковим.")
            .Equal(x => x.Password)
            .WithMessage(
                "Паролі не співпадають.");

        RuleFor(x => x.FirstName)
            .ValidFirstName();

        RuleFor(x => x.LastName)
            .ValidLastName();

        RuleFor(x => x.Position)
            .ValidPosition();
    }
}