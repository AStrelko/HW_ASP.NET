using HW_06.DTOs.MeetingDTO;
using HW_06.Validators.ResultsValid;

namespace HW_06.Validators.MeetingValid;

public class MeetingUpdateValidator : IValidator<MeetingUpdateDTO>
{
    public ValidationResult Validate(MeetingUpdateDTO model)
    {
        var result = new ValidationResult();

        ValidateMeetingId(model, result);
        ValidateTitle(model, result);
        ValidateDescription(model, result);
        ValidateDate(model, result);
        ValidateRoom(model, result);
        ValidateParticipants(model, result);

        return result;
    }

    private static void ValidateMeetingId(MeetingUpdateDTO model, ValidationResult result)
    {
        if (model.MeetingId <= 0)
        {
            result.AddError(nameof(model.MeetingId),
                "НЕ вірний ID.");
        }
    }

    private static void ValidateTitle(MeetingUpdateDTO model, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(model.Title))
        {
            result.AddError(nameof(model.Title),
                "Назва зустрічі є обов’язковою.");
            return;
        }

        if (model.Title.Length < 3)
        {
            result.AddError(nameof(model.Title),
                "Назва зустрічі повинна містити щонайменше 3 символи.");
        }

        if (model.Title.Length > 100)
        {
            result.AddError(nameof(model.Title),
                "Назва зустрічі не може перевищувати 100 символів.");
        }
    }

    private static void ValidateDescription(MeetingUpdateDTO model, ValidationResult result)
    {
        if (!string.IsNullOrWhiteSpace(model.Description) &&
            model.Description.Length > 1000)
        {
            result.AddError(nameof(model.Description),
                "Опис не може перевищувати 1000 символів.");
        }
    }

    private static void ValidateDate(MeetingUpdateDTO model, ValidationResult result)
    {
        if (model.DateTime < DateTime.Now)
        {
            result.AddError(nameof(model.DateTime),
                "Дата зустрічі не може бути в минулому.");
        }
    }

    /// <summary>
    /// Перевіряє номер кімнати.
    /// </summary>
    /// <param name="model">DTO повного оновлення зустрічі.</param>
    /// <param name="result">Результат валідації.</param>
    private static void ValidateRoom(
        MeetingUpdateDTO model,
        ValidationResult result)
    {
        if (model.RoomNumber is not null &&
            model.RoomNumber <= 0)
        {
            result.AddError(
                nameof(model.RoomNumber),
                "Необхідно вказати коректний номер кімнати.");
        }
    }

    private static void ValidateParticipants(MeetingUpdateDTO model, ValidationResult result)
    {
        if (model.ParticipantIds.Any(id => id <= 0))
        {
            result.AddError(nameof(model.ParticipantIds),
                "Ідентифікатори учасників повинні бути більшими за нуль.");
        }

        if (model.ParticipantIds.Count != model.ParticipantIds.Distinct().Count())
        {
            result.AddError(nameof(model.ParticipantIds),
                "Список учасників не повинен містити повторювані ідентифікатори.");
        }
    }
}