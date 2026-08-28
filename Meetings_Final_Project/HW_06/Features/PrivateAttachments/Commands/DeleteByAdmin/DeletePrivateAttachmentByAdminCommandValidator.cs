using FluentValidation;

namespace HW_06.Features.PrivateAttachments.Commands.DeleteByAdmin;

/// <summary>
/// Виконує перевірку команди
/// видалення приватного файлу адміністратором.
/// </summary>
public class DeletePrivateAttachmentByAdminCommandValidator
    : AbstractValidator<
        DeletePrivateAttachmentByAdminCommand>
{
    public DeletePrivateAttachmentByAdminCommandValidator()
    {
        RuleFor(command =>
                command.FileId)
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатор файлу повинен бути більшим за нуль.");
    }
}