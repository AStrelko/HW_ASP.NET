using HW_06.DTOs.MeetingDTO;
using HW_06.Validators.ResultsValid;

namespace HW_06.Validators.MeetingValid;

public class MeetingPartialUpdateValidator : IValidator<MeetingPartialUpdateDTO>
{
    public ValidationResult Validate(MeetingPartialUpdateDTO model)
    {
        var result = new ValidationResult();

        ValidateTitle(model, result);
        ValidateDescription(model, result);
        ValidateDate(model, result);
        ValidateRoom(model, result);

        return result;
    }

    private static void ValidateTitle(MeetingPartialUpdateDTO model, ValidationResult result)
    {
        if (model.Title is null)
            return;

        if (string.IsNullOrWhiteSpace(model.Title))
        {
            result.AddError(nameof(model.Title),
                "Назва зустрічі не може бути порожньою.");
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

    private static void ValidateDescription(MeetingPartialUpdateDTO model, ValidationResult result)
    {
        if (model.Description is null)
            return;

        if (model.Description.Length > 1000)
        {
            result.AddError(nameof(model.Description),
                "Опис не може перевищувати 1000 символів.");
        }
    }

    private static void ValidateDate(MeetingPartialUpdateDTO model, ValidationResult result)
    {
        if (!model.DateTime.HasValue)
            return;

        if (model.DateTime.Value < DateTime.Now)
        {
            result.AddError(nameof(model.DateTime),
                "Дата зустрічі не може бути в минулому.");
        }
    }

    /// <summary>
    /// Перевіряє новий номер кімнати.
    /// </summary>
    /// <param name="model">DTO часткового оновлення зустрічі.</param>
    /// <param name="result">Результат валідації.</param>
    private static void ValidateRoom(
        MeetingPartialUpdateDTO model,
        ValidationResult result)
    {
        if (!model.RoomNumber.HasValue)
        {
            return;
        }

        if (model.RoomNumber.Value <= 0)
        {
            result.AddError(
                nameof(model.RoomNumber),
                "Необхідно вказати коректний номер кімнати.");
        }
    }
}