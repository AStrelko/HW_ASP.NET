using FluentValidation;
using HW_06.Features.Participants.Common;

namespace HW_06.Features.Participants.Commands.Update;

/// <summary>
/// Виконує перевірку даних
/// команди повного оновлення учасника.
/// </summary>
public class UpdateParticipantCommandValidator
    : AbstractValidator<UpdateParticipantCommand>
{
    public UpdateParticipantCommandValidator()
    {
        RuleFor(command =>
                command.Id)
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатор учасника повинен бути більшим за нуль.");

        RuleFor(command =>
                command.Dto.FirstName)
            .NotEmpty()
            .WithMessage(
                "Ім’я є обов’язковим.")
            .ValidFirstName();

        RuleFor(command =>
                command.Dto.LastName)
            .NotEmpty()
            .WithMessage(
                "Прізвище є обов’язковим.")
            .ValidLastName();

        RuleFor(command =>
                command.Dto.Position)
            .ValidPosition();

        RuleFor(command =>
                command.Dto.MeetingIds)
            .NotNull()
            .WithMessage(
                "Список зустрічей є обов’язковим.")
            .ValidMeetingIds();
    }
}