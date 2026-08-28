using FluentValidation;

namespace HW_06.Features.Meetings.Commands.DeleteMany;

/// <summary>
/// Виконує перевірку команди
/// масового видалення зустрічей.
/// </summary>
public class DeleteManyMeetingsCommandValidator
    : AbstractValidator<DeleteManyMeetingsCommand>
{
    public DeleteManyMeetingsCommandValidator()
    {
        RuleFor(command =>
                command.Ids)
            .NotNull()
            .WithMessage(
                "Список ідентифікаторів є обов’язковим.")
            .NotEmpty()
            .WithMessage(
                "Список ідентифікаторів не може бути порожнім.");

        RuleForEach(command =>
                command.Ids)
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатори зустрічей повинні бути більшими за нуль.");

        RuleFor(command =>
                command.Ids)
            .Must(ids =>
                ids is null ||
                ids.Distinct().Count() ==
                ids.Count)
            .WithMessage(
                "Список не повинен містити повторювані ідентифікатори.");
    }
}