using FluentValidation;
using HW_06.DTOs.ParticipantDTO;

namespace HW_06.Validators.ParticipantValid;

/// <summary>
/// Виконує перевірку даних,
/// необхідних для повного оновлення учасника.
/// </summary>
public class ParticipantUpdateValidator
    : AbstractValidator<ParticipantUpdateDTO>
{
    public ParticipantUpdateValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("Ім’я є обов’язковим.")
            .ValidFirstName();

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Прізвище є обов’язковим.")
            .ValidLastName();

        RuleFor(x => x.Position)
            .ValidPosition();

        RuleFor(x => x.MeetingIds)
            .NotNull()
            .WithMessage(
                "Список зустрічей є обов’язковим для повного оновлення.")
            .ValidMeetingIds();
    }
}