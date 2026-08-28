using FluentValidation;

namespace HW_06.Features.Attachments.Commands.Delete;

/// <summary>
/// Виконує перевірку команди
/// видалення публічного файлу.
/// </summary>
public class DeleteAttachmentCommandValidator
    : AbstractValidator<DeleteAttachmentCommand>
{
    public DeleteAttachmentCommandValidator()
    {
        RuleFor(command =>
                command.MeetingId)
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатор зустрічі повинен бути більшим за нуль.");

        RuleFor(command =>
                command.AttachmentId)
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатор файлу повинен бути більшим за нуль.");
    }
}