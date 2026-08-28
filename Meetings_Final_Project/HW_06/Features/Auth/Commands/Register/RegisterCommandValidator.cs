using FluentValidation;
using HW_06.Features.Common.Identity;

namespace HW_06.Features.Auth.Commands.Register;

/// <summary>
/// Виконує перевірку даних
/// команди реєстрації користувача.
/// </summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(command =>
                command.Dto.Email)
            .ValidEmail();

        RuleFor(command =>
                command.Dto.Password)
            .ValidPassword();

        RuleFor(command =>
                command.Dto.ConfirmPassword)
            .NotEmpty()
            .WithMessage(
                "Підтвердження пароля є обов’язковим.")
            .Equal(command =>
                command.Dto.Password)
            .WithMessage(
                "Паролі не співпадають.");

        RuleFor(command =>
                command.Dto.FirstName)
            .ValidFirstName();

        RuleFor(command =>
                command.Dto.LastName)
            .ValidLastName();

        RuleFor(command =>
                command.Dto.Position)
            .ValidPosition();
    }
}