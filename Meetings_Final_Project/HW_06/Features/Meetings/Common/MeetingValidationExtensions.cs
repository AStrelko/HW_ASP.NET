using FluentValidation;

namespace HW_06.Features.Meetings.Common;

/// <summary>
/// Містить спільні правила валідації
/// для операцій із зустрічами.
/// </summary>
public static class MeetingValidationExtensions
{
    /// <summary>
    /// Перевіряє коректність назви зустрічі.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidMeetingTitle<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MinimumLength(3)
            .WithMessage(
                "Назва зустрічі повинна містити щонайменше 3 символи.")
            .MaximumLength(100)
            .WithMessage(
                "Назва зустрічі не може перевищувати 100 символів.");
    }

    /// <summary>
    /// Перевіряє коректність опису зустрічі.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidMeetingDescription<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(1000)
            .WithMessage(
                "Опис не може перевищувати 1000 символів.");
    }

    /// <summary>
    /// Перевіряє дату та час зустрічі.
    /// </summary>
    public static IRuleBuilderOptions<T, DateTime> ValidMeetingDate<T>(
        this IRuleBuilder<T, DateTime> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(DateTime.Now)
            .WithMessage(
                "Дата зустрічі не може бути в минулому.");
    }

    /// <summary>
    /// Перевіряє необов'язкову дату та час зустрічі
    /// під час часткового оновлення.
    /// </summary>
    public static IRuleBuilderOptions<T, DateTime?> ValidOptionalMeetingDate<T>(
        this IRuleBuilder<T, DateTime?> ruleBuilder)
    {
        return ruleBuilder
            .Must(date =>
                date is null ||
                date.Value >= DateTime.Now)
            .WithMessage(
                "Дата зустрічі не може бути в минулому.");
    }

    /// <summary>
    /// Перевіряє номер кімнати.
    /// </summary>
    public static IRuleBuilderOptions<T, int?> ValidRoomNumber<T>(
        this IRuleBuilder<T, int?> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0)
            .WithMessage(
                "Номер кімнати повинен бути більшим за нуль.");
    }

    /// <summary>
    /// Перевіряє список ідентифікаторів учасників.
    /// </summary>
    public static IRuleBuilderOptions<T, List<int>?> ValidParticipantIds<T>(
        this IRuleBuilder<T, List<int>?> ruleBuilder)
    {
        return ruleBuilder
            .Must(ids =>
                ids is null ||
                ids.All(id => id > 0))
            .WithMessage(
                "Ідентифікатори учасників повинні бути більшими за нуль.")
            .Must(ids =>
                ids is null ||
                ids.Distinct().Count() == ids.Count)
            .WithMessage(
                "Список учасників не повинен містити повторювані ідентифікатори.");
    }
}