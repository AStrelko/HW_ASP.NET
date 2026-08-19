using FluentValidation;
using HW_06.DTOs.IdentityDTO;

namespace HW_06.Validators.IdentityValid;

/// <summary>
/// Виконує перевірку даних
/// для входу користувача в систему.
/// </summary>
public class LoginDTOValidator
    : AbstractValidator<LoginDTO>
{
    public LoginDTOValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(
                "Електронна пошта є обов’язковою.")
            .EmailAddress()
            .WithMessage(
                "Вказано некоректну адресу електронної пошти.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(
                "Пароль є обов’язковим.");
    }
}