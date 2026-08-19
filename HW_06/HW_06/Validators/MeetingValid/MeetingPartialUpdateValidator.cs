using FluentValidation;
using HW_06.DTOs.MeetingDTO;

namespace HW_06.Validators.MeetingValid;

/// <summary>
/// Виконує перевірку даних,
/// необхідних для часткового оновлення зустрічі.
/// </summary>
public class MeetingPartialUpdateValidator
    : AbstractValidator<MeetingPartialUpdateDTO>
{
    public MeetingPartialUpdateValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Назва зустрічі не може бути порожньою.")
            .ValidMeetingTitle()
            .When(x => x.Title is not null);

        RuleFor(x => x.Description)
            .ValidMeetingDescription()
            .When(x => x.Description is not null);

        RuleFor(x => x.DateTime)
            .ValidOptionalMeetingDate();

        RuleFor(x => x.RoomNumber)
            .ValidRoomNumber()
            .When(x => x.RoomNumber.HasValue);
    }
}