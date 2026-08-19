using FluentValidation;
using HW_06.DTOs.ParticipantDTO;

namespace HW_06.Validators.ParticipantValid;

/// <summary>
/// Виконує перевірку даних,
/// необхідних для часткового оновлення учасника.
/// </summary>
public class ParticipantPartialUpdateValidator
    : AbstractValidator<ParticipantPartialUpdateDTO>
{
    public ParticipantPartialUpdateValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("Ім’я не може бути порожнім.")
            .ValidFirstName()
            .When(x => x.FirstName is not null);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Прізвище не може бути порожнім.")
            .ValidLastName()
            .When(x => x.LastName is not null);

        RuleFor(x => x.Position)
            .ValidPosition()
            .When(x => x.Position is not null);

        RuleFor(x => x.MeetingIds)
            .ValidMeetingIds()
            .When(x => x.MeetingIds is not null);
    }
}