using FluentValidation;

namespace HW_06.Features.Meetings.Commands.Delete;

/// <summary>
/// Виконує перевірку команди
/// видалення зустрічі.
/// </summary>
public class DeleteMeetingCommandValidator
    : AbstractValidator<DeleteMeetingCommand>
{
    public DeleteMeetingCommandValidator()
    {
        RuleFor(command =>
                command.Id)
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатор зустрічі повинен бути більшим за нуль.");
    }
}