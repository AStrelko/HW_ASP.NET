using FluentValidation;
using HW_06.Features.Meetings.Common;

namespace HW_06.Features.Meetings.Commands.Update;

/// <summary>
/// Виконує перевірку команди
/// повного оновлення зустрічі.
/// </summary>
public class UpdateMeetingCommandValidator
    : AbstractValidator<UpdateMeetingCommand>
{
    public UpdateMeetingCommandValidator()
    {
        RuleFor(command =>
                command.Id)
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатор зустрічі повинен бути більшим за нуль.");

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