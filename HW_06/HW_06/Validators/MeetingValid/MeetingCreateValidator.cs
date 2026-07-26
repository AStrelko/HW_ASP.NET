using HW_06.DTOs.MeetingDTO;
using HW_06.Validators;
using HW_06.Validators.ResultsValid;

namespace HW_06.Validators.MeetingValid;

public class MeetingCreateValidator : IValidator<MeetingCreateDTO>
{
    public ValidationResult Validate(MeetingCreateDTO model)
    {
        var result = new ValidationResult();

        ValidateTitle(model, result);
        ValidateDescription(model, result);
        ValidateDate(model, result);
        ValidateRoom(model, result);
        ValidateParticipants(model, result);

        return result;
    }
    
    private static void ValidateTitle(MeetingCreateDTO model, ValidationResult result)
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

    private static void ValidateDescription(MeetingCreateDTO model, ValidationResult result)
    {
        if (!string.IsNullOrWhiteSpace(model.Description) &&
            model.Description.Length > 1000)
        {
            result.AddError(nameof(model.Description),
                "Опис не може перевищувати 1000 символів.");
        }
    }

    private static void ValidateDate(MeetingCreateDTO model, ValidationResult result)
    {
        if (model.DateTime < DateTime.Now)
        {
            result.AddError(nameof(model.DateTime),
                "Дата зустрічі не може бути в минулому.");
        }
    }

    private static void ValidateRoom(MeetingCreateDTO model, ValidationResult result)
    {
        if (model.RoomId is null || model.RoomId <= 0)
        {
            result.AddError(nameof(model.RoomId),
                "Необхідно вибрати коректну кімнату.");
        }
    }

    private static void ValidateParticipants(MeetingCreateDTO model, ValidationResult result)
    {
        if (model.ParticipantIds.Count == 0)
        {
            result.AddError(nameof(model.ParticipantIds),
                "Необхідно додати щонайменше одного учасника.");
        }

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