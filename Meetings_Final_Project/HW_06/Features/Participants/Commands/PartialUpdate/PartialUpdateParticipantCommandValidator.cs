using FluentValidation;
using HW_06.Features.Participants.Common;

namespace HW_06.Features.Participants.Commands.PartialUpdate;

/// <summary>
/// Виконує перевірку даних
/// команди часткового оновлення учасника.
/// </summary>
public class PartialUpdateParticipantCommandValidator
    : AbstractValidator<PartialUpdateParticipantCommand>
{
    public PartialUpdateParticipantCommandValidator()
    {
        RuleFor(command =>
                command.Id)
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатор учасника повинен бути більшим за нуль.");

        RuleFor(command =>
                command.Dto.FirstName)
            .ValidFirstName()
            .When(command =>
                command.Dto.FirstName is not null);

        RuleFor(command =>
                command.Dto.LastName)
            .ValidLastName()
            .When(command =>
                command.Dto.LastName is not null);

        RuleFor(command =>
                command.Dto.Position)
            .ValidPosition()
            .When(command =>
                command.Dto.Position is not null);

        RuleFor(command =>
                command.Dto.MeetingIds)
            .ValidMeetingIds()
            .When(command =>
                command.Dto.MeetingIds is not null);
    }
}