using FluentValidation;

namespace HW_06.Features.Participants.Commands.DeleteMany;

/// <summary>
/// Виконує перевірку команди
/// масового видалення учасників.
/// </summary>
public class DeleteManyParticipantsCommandValidator
    : AbstractValidator<DeleteManyParticipantsCommand>
{
    public DeleteManyParticipantsCommandValidator()
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
                "Ідентифікатори учасників повинні бути більшими за нуль.");

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