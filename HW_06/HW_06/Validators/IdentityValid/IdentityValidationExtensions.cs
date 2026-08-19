using FluentValidation;

namespace HW_06.Validators.IdentityValid;

public static class IdentityValidationExtensions
{
    public static IRuleBuilderOptions<T, string> ValidEmail<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Електронна пошта є обов’язковою.")
            .EmailAddress()
            .WithMessage("Вказано некоректну адресу електронної пошти.");
    }

    public static IRuleBuilderOptions<T, string> ValidPassword<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Пароль є обов’язковим.")
            .MinimumLength(6)
            .WithMessage("Пароль повинен містити щонайменше 6 символів.")
            .Matches(@"[0-9]")
            .WithMessage("Пароль повинен містити хоча б одну цифру.");
    }

    public static IRuleBuilderOptions<T, string> ValidFirstName<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Ім’я є обов’язковим.")
            .MinimumLength(2)
            .WithMessage("Ім’я повинно містити щонайменше 2 символи.")
            .MaximumLength(50)
            .WithMessage("Ім’я не може перевищувати 50 символів.");
    }

    public static IRuleBuilderOptions<T, string> ValidLastName<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Прізвище є обов’язковим.")
            .MinimumLength(2)
            .WithMessage("Прізвище повинно містити щонайменше 2 символи.")
            .MaximumLength(50)
            .WithMessage("Прізвище не може перевищувати 50 символів.");
    }

    public static IRuleBuilderOptions<T, string?> ValidPosition<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(50)
            .WithMessage("Посада не може перевищувати 50 символів.");
    }
}