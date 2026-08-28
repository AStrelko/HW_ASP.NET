using FluentValidation;
using HW_06.Features.Meetings.Common;

namespace HW_06.Features.Meetings.Commands.PartialUpdate;

/// <summary>
/// Виконує перевірку даних
/// команди часткового оновлення зустрічі.
/// </summary>
public class PartialUpdateMeetingCommandValidator
    : AbstractValidator<PartialUpdateMeetingCommand>
{
    public PartialUpdateMeetingCommandValidator()
    {
        RuleFor(command =>
                command.Id)
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатор зустрічі повинен бути більшим за нуль.");

        RuleFor(command =>
                command.Dto.Title)
            .ValidMeetingTitle()
            .When(command =>
                command.Dto.Title is not null);

        RuleFor(command =>
                command.Dto.Description)
            .ValidMeetingDescription()
            .When(command =>
                command.Dto.Description is not null);

        RuleFor(command =>
                command.Dto.DateTime)
            .ValidOptionalMeetingDate();

        RuleFor(command =>
                command.Dto.RoomNumber)
            .ValidRoomNumber()
            .When(command =>
                command.Dto.RoomNumber.HasValue);
    }
}