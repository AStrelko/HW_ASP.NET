using FluentValidation;
using HW_06.Common.Constants;

namespace HW_06.Features.Common.Identity;

/// <summary>
/// Містить спільні правила валідації
/// для даних автентифікації, користувачів та ролей.
/// </summary>
public static class IdentityValidationExtensions
{
    /// <summary>
    /// Перевіряє коректність електронної пошти.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidEmail<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage(
                "Електронна пошта є обов’язковою.")
            .EmailAddress()
            .WithMessage(
                "Вказано некоректну адресу електронної пошти.");
    }

    /// <summary>
    /// Перевіряє коректність пароля.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidPassword<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage(
                "Пароль є обов’язковим.")
            .MinimumLength(6)
            .WithMessage(
                "Пароль повинен містити щонайменше 6 символів.")
            .Matches(@"[0-9]")
            .WithMessage(
                "Пароль повинен містити хоча б одну цифру.");
    }

    /// <summary>
    /// Перевіряє коректність імені.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidFirstName<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage(
                "Ім’я є обов’язковим.")
            .MinimumLength(2)
            .WithMessage(
                "Ім’я повинно містити щонайменше 2 символи.")
            .MaximumLength(50)
            .WithMessage(
                "Ім’я не може перевищувати 50 символів.");
    }

    /// <summary>
    /// Перевіряє коректність прізвища.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidLastName<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage(
                "Прізвище є обов’язковим.")
            .MinimumLength(2)
            .WithMessage(
                "Прізвище повинно містити щонайменше 2 символів.")
            .MaximumLength(50)
            .WithMessage(
                "Прізвище не може перевищувати 50 символів.");
    }

    /// <summary>
    /// Перевіряє коректність посади
    /// або спеціалізації.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidPosition<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(50)
            .WithMessage(
                "Посада не може перевищувати 50 символів.")
            .Must(position =>
                string.IsNullOrWhiteSpace(position) ||
                ParticipantPositions.All.Contains(
                    position,
                    StringComparer.OrdinalIgnoreCase))
            .WithMessage(
                $"Дозволені лише посади: " +
                $"{string.Join(", ", ParticipantPositions.All)}.");
    }

    /// <summary>
    /// Перевіряє коректність назви ролі.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidRoleName<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage(
                "Назва ролі є обов’язковою.")
            .Must(role =>
                ApplicationRoles.All.Contains(
                    role,
                    StringComparer.OrdinalIgnoreCase))
            .WithMessage(
                $"Дозволені лише ролі: " +
                $"{string.Join(", ", ApplicationRoles.All)}.");
    }

    /// <summary>
    /// Перевіряє коректність
    /// ідентифікатора учасника.
    /// </summary>
    public static IRuleBuilderOptions<T, int> ValidParticipantId<T>(
        this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатор учасника повинен бути більшим за нуль.");
    }
}