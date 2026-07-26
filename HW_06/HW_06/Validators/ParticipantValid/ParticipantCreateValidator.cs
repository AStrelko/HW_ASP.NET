using HW_06.DTOs.ParticipantDTO;
using HW_06.Validators.ResultsValid;
using System.Net.Mail;

namespace HW_06.Validators.ParticipantValid;

public class ParticipantCreateValidator : IValidator<ParticipantCreateDTO>
{
    public ValidationResult Validate(ParticipantCreateDTO model)
    {
        var result = new ValidationResult();

        ValidateFirstName(model, result);
        ValidateLastName(model, result);
        ValidateEmail(model, result);
        ValidateRole(model, result);
        ValidateListMeeting(model, result);

        return result;
    }

    private static void ValidateFirstName(ParticipantCreateDTO model, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(model.FirstName))
        {
            result.AddError(nameof(model.FirstName), "Ім’я є обов’язковим.");
            return;
        }

        if (model.FirstName.Length < 2)
            result.AddError(nameof(model.FirstName), "Ім’я повинно містити щонайменше 2 символи.");

        if (model.FirstName.Length > 50)
            result.AddError(nameof(model.FirstName), "Ім’я повинно містити щонайбільш 50 символів.");
    }

    private static void ValidateLastName(ParticipantCreateDTO model, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(model.LastName))
        {
            result.AddError(nameof(model.LastName), "Прізвище є обов’язковим.");
            return;
        }

        if (model.LastName.Length < 2)
            result.AddError(nameof(model.LastName), "Прізвище повинно містити щонайменше 2 символи.");

        if (model.LastName.Length > 50)
            result.AddError(nameof(model.LastName), "Прізвище не може перевищувати 50 символів.");
    }

    private static void ValidateEmail(ParticipantCreateDTO model, ValidationResult result)
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

    private static void ValidateRole(ParticipantCreateDTO model, ValidationResult result)
    {
        if (!string.IsNullOrWhiteSpace(model.Role) && model.Role.Length > 50)
        {
            result.AddError(nameof(model.Role), "Роль не може перевищувати 50 символів.");
        }
    }

    private static void ValidateListMeeting(ParticipantCreateDTO model, ValidationResult result)
    {
        if (model.MeetingIds is null)
        {
            return;
        }

        if (model.MeetingIds.Any(id => id <= 0))
        {
            result.AddError(nameof(model.MeetingIds), "Ідентифікатори зустрічей повинні бути більшими за нуль.");
        }

        if (model.MeetingIds.Count != model.MeetingIds.Distinct().Count())
        {
            result.AddError(nameof(model.MeetingIds), "Список зустрічей містить повторювані ідентифікатори.");
        }
    }
}