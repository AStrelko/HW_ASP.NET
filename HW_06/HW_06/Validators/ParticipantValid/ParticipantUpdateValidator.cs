using HW_06.DTOs.ParticipantDTO;
using HW_06.Validators.ResultsValid;
using System.Net.Mail;

namespace HW_06.Validators.ParticipantValid;

/// <summary>
/// Виконує перевірку даних,
/// необхідних для повного оновлення учасника.
/// </summary>
public class ParticipantUpdateValidator
    : IValidator<ParticipantUpdateDTO>
{
    /// <summary>
    /// Виконує комплексну перевірку
    /// даних повного оновлення учасника.
    /// </summary>
    /// <param name="model">
    /// DTO з даними для повного оновлення учасника.
    /// </param>
    /// <returns>
    /// Результат валідації, що містить
    /// інформацію про виявлені помилки.
    /// </returns>
    public ValidationResult Validate(
        ParticipantUpdateDTO model)
    {
        var result = new ValidationResult();

        ValidateParticipantId(model, result);
        ValidateFirstName(model, result);
        ValidateLastName(model, result);
        ValidateEmail(model, result);
        ValidateRole(model, result);
        ValidateMeetingIds(model, result);

        return result;
    }

    /// <summary>
    /// Перевіряє коректність ідентифікатора учасника.
    /// </summary>
    /// <param name="model">
    /// DTO повного оновлення учасника.
    /// </param>
    /// <param name="result">
    /// Результат валідації, до якого додаються виявлені помилки.
    /// </param>
    private static void ValidateParticipantId(
        ParticipantUpdateDTO model,
        ValidationResult result)
    {
        if (model.ParticipantId <= 0)
        {
            result.AddError(
                nameof(model.ParticipantId),
                "Ідентифікатор учасника повинен бути більшим за нуль.");
        }
    }

    /// <summary>
    /// Перевіряє коректність імені учасника.
    /// </summary>
    /// <param name="model">
    /// DTO повного оновлення учасника.
    /// </param>
    /// <param name="result">
    /// Результат валідації, до якого додаються виявлені помилки.
    /// </param>
    private static void ValidateFirstName(
        ParticipantUpdateDTO model,
        ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(model.FirstName))
        {
            result.AddError(
                nameof(model.FirstName),
                "Ім’я не може бути порожнім.");

            return;
        }

        if (model.FirstName.Length < 2)
        {
            result.AddError(
                nameof(model.FirstName),
                "Ім’я повинно містити щонайменше 2 символи.");
        }

        if (model.FirstName.Length > 50)
        {
            result.AddError(
                nameof(model.FirstName),
                "Ім’я не може перевищувати 50 символів.");
        }
    }

    /// <summary>
    /// Перевіряє коректність прізвища учасника.
    /// </summary>
    /// <param name="model">
    /// DTO повного оновлення учасника.
    /// </param>
    /// <param name="result">
    /// Результат валідації, до якого додаються виявлені помилки.
    /// </param>
    private static void ValidateLastName(
        ParticipantUpdateDTO model,
        ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(model.LastName))
        {
            result.AddError(
                nameof(model.LastName),
                "Прізвище не може бути порожнім.");

            return;
        }

        if (model.LastName.Length < 2)
        {
            result.AddError(
                nameof(model.LastName),
                "Прізвище повинно містити щонайменше 2 символи.");
        }

        if (model.LastName.Length > 50)
        {
            result.AddError(
                nameof(model.LastName),
                "Прізвище не може перевищувати 50 символів.");
        }
    }

    /// <summary>
    /// Перевіряє наявність і коректність
    /// адреси електронної пошти учасника.
    /// </summary>
    /// <param name="model">
    /// DTO повного оновлення учасника.
    /// </param>
    /// <param name="result">
    /// Результат валідації, до якого додаються виявлені помилки.
    /// </param>
    private static void ValidateEmail(
        ParticipantUpdateDTO model,
        ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(model.Email))
        {
            result.AddError(
                nameof(model.Email),
                "Електронна пошта є обов’язковою.");

            return;
        }

        try
        {
            _ = new MailAddress(model.Email);
        }
        catch
        {
            result.AddError(
                nameof(model.Email),
                "Вказано некоректну адресу електронної пошти.");
        }
    }

    /// <summary>
    /// Перевіряє коректність ролі учасника.
    /// </summary>
    /// <param name="model">
    /// DTO повного оновлення учасника.
    /// </param>
    /// <param name="result">
    /// Результат валідації, до якого додаються виявлені помилки.
    /// </param>
    private static void ValidateRole(
        ParticipantUpdateDTO model,
        ValidationResult result)
    {
        if (!string.IsNullOrWhiteSpace(model.Role) &&
            model.Role.Length > 50)
        {
            result.AddError(
                nameof(model.Role),
                "Роль не може перевищувати 50 символів.");
        }
    }

    /// <summary>
    /// Перевіряє коректність списку
    /// ідентифікаторів зустрічей учасника.
    /// </summary>
    /// <param name="model">
    /// DTO повного оновлення учасника.
    /// </param>
    /// <param name="result">
    /// Результат валідації, до якого додаються виявлені помилки.
    /// </param>
    private static void ValidateMeetingIds(
        ParticipantUpdateDTO model,
        ValidationResult result)
    {
        if (model.MeetingIds is null)
        {
            result.AddError(
                nameof(model.MeetingIds),
                "Список зустрічей є обов’язковим для повного оновлення.");

            return;
        }

        if (model.MeetingIds.Any(id => id <= 0))
        {
            result.AddError(
                nameof(model.MeetingIds),
                "Ідентифікатори зустрічей повинні бути більшими за нуль.");
        }

        if (model.MeetingIds.Count !=
            model.MeetingIds.Distinct().Count())
        {
            result.AddError(
                nameof(model.MeetingIds),
                "Список зустрічей містить повторювані ідентифікатори.");
        }
    }
}