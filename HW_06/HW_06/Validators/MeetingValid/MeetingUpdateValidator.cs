using FluentValidation;
using HW_06.DTOs.MeetingDTO;

namespace HW_06.Validators.MeetingValid;

/// <summary>
/// Виконує перевірку даних,
/// необхідних для повного оновлення зустрічі.
/// </summary>
public class MeetingUpdateValidator
    : AbstractValidator<MeetingUpdateDTO>
{
    public MeetingUpdateValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Назва зустрічі є обов’язковою.")
            .ValidMeetingTitle();

        RuleFor(x => x.Description)
            .ValidMeetingDescription();

        RuleFor(x => x.DateTime)
            .ValidMeetingDate();

        RuleFor(x => x.RoomNumber)
            .NotNull()
            .WithMessage("Необхідно вказати номер кімнати.")
            .ValidRoomNumber();

        RuleFor(x => x.ParticipantIds)
            .NotEmpty()
            .WithMessage(
                "Необхідно додати щонайменше одного учасника.")
            .ValidParticipantIds();
    }
}