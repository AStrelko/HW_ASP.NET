using HW_06.DTOs.ParticipantDTO;
using HW_06.Validators.ResultsValid;
using System.Net.Mail;

namespace HW_06.Validators.ParticipantValid;

public class ParticipantUpdateValidator : IValidator<ParticipantUpdateDTO>
{
    public ValidationResult Validate(ParticipantUpdateDTO model)
    {
        var result = new ValidationResult();

        if (model.ParticipantId <= 0)
        {
            result.AddError(
                nameof(model.ParticipantId),
                "Ідентифікатор учасника повинен бути більшим за нуль.");
        }

        ValidateFirstName(model, result);
        ValidateLastName(model, result);
        ValidateEmail(model, result);
        ValidateRole(model, result);
        ValidateMeetingIds(model, result);

        return result;
    }

    private static void ValidateFirstName(ParticipantUpdateDTO model, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(model.FirstName))
        {
            result.AddError(nameof(model.FirstName), "Ім’я не може бути порожнім.");
            return;
        }

        if (model.FirstName.Length < 2)
            result.AddError(nameof(model.FirstName), "Ім’я повинно містити щонайменше 2 символи.");

        if (model.FirstName.Length > 50)
            result.AddError(nameof(model.FirstName), "Ім’я повинно містити щонайбільш 50 символів.");
    }

    private static void ValidateLastName(ParticipantUpdateDTO model, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(model.LastName))
        {
            result.AddError(nameof(model.LastName), "Прізвище не може бути порожнім.");
            return;
        }

        if (model.LastName.Length < 2)
            result.AddError(nameof(model.LastName), "Прізвище повинно містити щонайменше 2 символи.");

        if (model.LastName.Length > 50)
            result.AddError(nameof(model.LastName), "Прізвище не може перевищувати 50 символів.");
    }

    private static void ValidateEmail(ParticipantUpdateDTO model, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(model.Email))
        {
            result.AddError(nameof(model.Email), "Електронна пошта є обов’язковою.");
            return;
        }

        try
        {
            _ = new MailAddress(model.Email);
        }
        catch
        {
            result.AddError(nameof(model.Email), "Вказано некоректну адресу електронної пошти.");
        }
    }

    private static void ValidateRole(ParticipantUpdateDTO model, ValidationResult result)
    {
        if (!string.IsNullOrWhiteSpace(model.Role) && model.Role.Length > 50)
        {
            result.AddError(nameof(model.Role), "Роль не може перевищувати 50 символів.");
        }
    }
    
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