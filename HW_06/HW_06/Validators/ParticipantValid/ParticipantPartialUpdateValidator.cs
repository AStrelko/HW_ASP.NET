using HW_06.DTOs.ParticipantDTO;
using HW_06.Validators.ResultsValid;
using System.Net.Mail;

namespace HW_06.Validators.ParticipantValid;

public class ParticipantPartialUpdateValidator
    : IValidator<ParticipantPartialUpdateDTO>
{
    public ValidationResult Validate(
        ParticipantPartialUpdateDTO model)
    {
        var result = new ValidationResult();

        if (model.FirstName is not null)
        {
            if (string.IsNullOrWhiteSpace(model.FirstName))
            {
                result.AddError(
                    nameof(model.FirstName),
                    "Ім’я не може бути порожнім.");
            }
            else if (model.FirstName.Length < 2)
            {
                result.AddError(
                    nameof(model.FirstName),
                    "Ім’я повинно містити щонайменше 2 символи.");
            }
            else if (model.FirstName.Length > 50)
            {
                result.AddError(
                    nameof(model.FirstName),
                    "Ім’я не може перевищувати 50 символів.");
            }
        }

        if (model.LastName is not null)
        {
            if (string.IsNullOrWhiteSpace(model.LastName))
            {
                result.AddError(
                    nameof(model.LastName),
                    "Прізвище не може бути порожнім.");
            }
            else if (model.LastName.Length < 2)
            {
                result.AddError(
                    nameof(model.LastName),
                    "Прізвище повинно містити щонайменше 2 символи.");
            }
            else if (model.LastName.Length > 50)
            {
                result.AddError(
                    nameof(model.LastName),
                    "Прізвище не може перевищувати 50 символів.");
            }
        }

        if (model.Email is not null)
        {
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                result.AddError(
                    nameof(model.Email),
                    "Електронна пошта не може бути порожньою.");
            }
            else
            {
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
        }

        if (model.Role is not null &&
            model.Role.Length > 50)
        {
            result.AddError(
                nameof(model.Role),
                "Роль не може перевищувати 50 символів.");
        }
        
        ValidateMeetingIds(model, result);

        return result;
    }
    
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