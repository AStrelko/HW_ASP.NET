using HW_06.DTOs.ParticipantDTO;
using HW_06.Validators.ResultsValid;
using System.Net.Mail;

namespace HW_06.Validators.ParticipantValid;

/// <summary>
/// Виконує перевірку даних,
/// необхідних для часткового оновлення учасника.
/// </summary>
public class ParticipantPartialUpdateValidator
    : IValidator<ParticipantPartialUpdateDTO>
{
    /// <summary>
    /// Виконує комплексну перевірку
    /// даних часткового оновлення учасника.
    /// </summary>
    /// <param name="model">
    /// DTO з даними для часткового оновлення учасника.
    /// </param>
    /// <returns>
    /// Результат валідації, що містить
    /// інформацію про виявлені помилки.
    /// </returns>
    public ValidationResult Validate(
        ParticipantPartialUpdateDTO model)
    {
        var result = new ValidationResult();

        ValidateFirstName(model, result);
        ValidateLastName(model, result);
        ValidateEmail(model, result);
        ValidateRole(model, result);
        ValidateMeetingIds(model, result);

        return result;
    }

    /// <summary>
    /// Перевіряє коректність імені учасника.
    /// </summary>
    /// <param name="model">
    /// DTO часткового оновлення учасника.
    /// </param>
    /// <param name="result">
    /// Результат валідації, до якого додаються виявлені помилки.
    /// </param>
    private static void ValidateFirstName(
        ParticipantPartialUpdateDTO model,
        ValidationResult result)
    {
        if (model.FirstName is null)
        {
            return;
        }

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
    /// DTO часткового оновлення учасника.
    /// </param>
    /// <param name="result">
    /// Результат валідації, до якого додаються виявлені помилки.
    /// </param>
    private static void ValidateLastName(
        ParticipantPartialUpdateDTO model,
        ValidationResult result)
    {
        if (model.LastName is null)
        {
            return;
        }

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
    /// Перевіряє коректність адреси
    /// електронної пошти учасника.
    /// </summary>
    /// <param name="model">
    /// DTO часткового оновлення учасника.
    /// </param>
    /// <param name="result">
    /// Результат валідації, до якого додаються виявлені помилки.
    /// </param>
    private static void ValidateEmail(
        ParticipantPartialUpdateDTO model,
        ValidationResult result)
    {
        if (model.Email is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(model.Email))
        {
            result.AddError(
                nameof(model.Email),
                "Електронна пошта не може бути порожньою.");

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
    /// DTO часткового оновлення учасника.
    /// </param>
    /// <param name="result">
    /// Результат валідації, до якого додаються виявлені помилки.
    /// </param>
    private static void ValidateRole(
        ParticipantPartialUpdateDTO model,
        ValidationResult result)
    {
        if (model.Role is null)
        {
            return;
        }

        if (model.Role.Length > 50)
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
    /// DTO часткового оновлення учасника.
    /// </param>
    /// <param name="result">
    /// Результат валідації, до якого додаються виявлені помилки.
    /// </param>
    private static void ValidateMeetingIds(
        ParticipantPartialUpdateDTO model,
        ValidationResult result)
    {
        if (model.MeetingIds is null)
        {
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