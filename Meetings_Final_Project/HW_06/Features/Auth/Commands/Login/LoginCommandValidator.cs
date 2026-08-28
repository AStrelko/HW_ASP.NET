using FluentValidation;
using HW_06.Features.Common.Identity;

namespace HW_06.Features.Auth.Commands.Login;

/// <summary>
/// Виконує перевірку даних
/// команди входу користувача.
/// </summary>
public class LoginCommandValidator
    : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command =>
                command.Dto.Email)
            .ValidEmail();

        RuleFor(command =>
                command.Dto.Password)
            .NotEmpty()
            .WithMessage(
                "Пароль є обов’язковим.");
    }
}