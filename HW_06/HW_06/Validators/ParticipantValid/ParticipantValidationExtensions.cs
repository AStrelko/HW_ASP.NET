using FluentValidation;

namespace HW_06.Validators.ParticipantValid;

/// <summary>
/// Містить спільні правила валідації
/// для DTO учасників.
/// </summary>
public static class ParticipantValidationExtensions
{
    /// <summary>
    /// Перевіряє коректність імені учасника.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidFirstName<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MinimumLength(2)
            .WithMessage(
                "Ім’я повинно містити щонайменше 2 символи.")
            .MaximumLength(50)
            .WithMessage(
                "Ім’я не може перевищувати 50 символів.");
    }

    /// <summary>
    /// Перевіряє коректність прізвища учасника.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidLastName<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MinimumLength(2)
            .WithMessage(
                "Прізвище повинно містити щонайменше 2 символи.")
            .MaximumLength(50)
            .WithMessage(
                "Прізвище не може перевищувати 50 символів.");
    }

    /// <summary>
    /// Перевіряє коректність посади
    /// або спеціалізації учасника.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidPosition<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(50)
            .WithMessage(
                "Посада не може перевищувати 50 символів.");
    }
    
    /// <summary>
    /// Перевіряє коректність списку
    /// ідентифікаторів зустрічей.
    /// </summary>
    public static IRuleBuilderOptions<T, List<int>?> ValidMeetingIds<T>(
        this IRuleBuilder<T, List<int>?> ruleBuilder)
    {
        return ruleBuilder
            .Must(ids =>
                ids is null ||
                ids.All(id => id > 0))
            .WithMessage(
                "Ідентифікатори зустрічей повинні бути більшими за нуль.")
            .Must(ids =>
                ids is null ||
                ids.Distinct().Count() == ids.Count)
            .WithMessage(
                "Список зустрічей не повинен містити повторювані ідентифікатори.");
    }
}