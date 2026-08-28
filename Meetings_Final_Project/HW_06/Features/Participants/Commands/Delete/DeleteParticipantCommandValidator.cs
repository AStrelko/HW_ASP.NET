using FluentValidation;

namespace HW_06.Features.Participants.Commands.Delete;

/// <summary>
/// Виконує перевірку команди
/// видалення учасника.
/// </summary>
public class DeleteParticipantCommandValidator
    : AbstractValidator<DeleteParticipantCommand>
{
    public DeleteParticipantCommandValidator()
    {
        RuleFor(command =>
                command.Id)
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатор учасника повинен бути більшим за нуль.");
    }
}