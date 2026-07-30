using HW_06.DTOs.MeetingDTO;
using HW_06.Validators.ResultsValid;

namespace HW_06.Validators.MeetingValid;

/// <summary>
/// Виконує перевірку даних,
/// необхідних для повного оновлення зустрічі.
/// </summary>
public class MeetingUpdateValidator : IValidator<MeetingUpdateDTO>
{
    /// <summary>
    /// Виконує комплексну перевірку
    /// даних повного оновлення зустрічі.
    /// </summary>
    /// <param name="model">
    /// DTO з даними для повного оновлення зустрічі.
    /// </param>
    /// <returns>
    /// Результат валідації, що містить
    /// інформацію про виявлені помилки.
    /// </returns>
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

    /// <summary>
    /// Перевіряє коректність ідентифікатора зустрічі.
    /// </summary>
    /// <param name="model">
    /// DTO повного оновлення зустрічі.
    /// </param>
    /// <param name="result">
    /// Результат валідації, до якого додаються виявлені помилки.
    /// </param>
    private static void ValidateMeetingId(MeetingUpdateDTO model, ValidationResult result)
    {
        if (model.MeetingId <= 0)
        {
            result.AddError(nameof(model.MeetingId),
                "НЕ вірний ID.");
        }
    }

    /// <summary>
    /// Перевіряє коректність назви зустрічі.
    /// </summary>
    /// <param name="model">
    /// DTO повного оновлення зустрічі.
    /// </param>
    /// <param name="result">
    /// Результат валідації, до якого додаються виявлені помилки.
    /// </param>
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

    /// <summary>
    /// Перевіряє коректність опису зустрічі.
    /// </summary>
    /// <param name="model">
    /// DTO повного оновлення зустрічі.
    /// </param>
    /// <param name="result">
    /// Результат валідації, до якого додаються виявлені помилки.
    /// </param>
    private static void ValidateDescription(MeetingUpdateDTO model, ValidationResult result)
    {
        if (!string.IsNullOrWhiteSpace(model.Description) &&
            model.Description.Length > 1000)
        {
            result.AddError(nameof(model.Description),
                "Опис не може перевищувати 1000 символів.");
        }
    }

    /// <summary>
    /// Перевіряє коректність дати та часу зустрічі.
    /// </summary>
    /// <param name="model">
    /// DTO повного оновлення зустрічі.
    /// </param>
    /// <param name="result">
    /// Результат валідації, до якого додаються виявлені помилки.
    /// </param>
    private static void ValidateDate(MeetingUpdateDTO model, ValidationResult result)
    {
        if (model.DateTime < DateTime.Now)
        {
            result.AddError(nameof(model.DateTime),
                "Дата зустрічі не може бути в минулому.");
        }
    }

    /// <summary>
    /// Перевіряє коректність номера кімнати.
    /// </summary>
    /// <param name="model">
    /// DTO повного оновлення зустрічі.
    /// </param>
    /// <param name="result">
    /// Результат валідації, до якого додаються виявлені помилки.
    /// </param>
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

    /// <summary>
    /// Перевіряє коректність списку учасників зустрічі.
    /// </summary>
    /// <param name="model">
    /// DTO повного оновлення зустрічі.
    /// </param>
    /// <param name="result">
    /// Результат валідації, до якого додаються виявлені помилки.
    /// </param>
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