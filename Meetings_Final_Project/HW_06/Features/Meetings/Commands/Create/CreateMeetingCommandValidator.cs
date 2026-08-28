using FluentValidation;
using HW_06.Features.Meetings.Common;

namespace HW_06.Features.Meetings.Commands.Create;

/// <summary>
/// Виконує перевірку даних
/// команди створення зустрічі.
/// </summary>
public class CreateMeetingCommandValidator
    : AbstractValidator<CreateMeetingCommand>
{
    public CreateMeetingCommandValidator()
    {
        RuleFor(command =>
                command.Dto.Title)
            .NotEmpty()
            .WithMessage(
                "Назва зустрічі є обов’язковою.")
            .ValidMeetingTitle();

        RuleFor(command =>
                command.Dto.Description)
            .ValidMeetingDescription();

        RuleFor(command =>
                command.Dto.DateTime)
            .ValidMeetingDate();

        RuleFor(command =>
                command.Dto.RoomNumber)
            .ValidRoomNumber();

        RuleFor(command =>
                command.Dto.ParticipantIds)
            .NotNull()
            .WithMessage(
                "Список учасників є обов’язковим.")
            .NotEmpty()
            .WithMessage(
                "Зустріч повинна містити хоча б одного учасника.")
            .ValidParticipantIds();
    }
}