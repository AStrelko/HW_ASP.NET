using FluentValidation;
using HW_06.Features.Common.Identity;

namespace HW_06.Features.PrivateAttachments.Commands.Delete;

/// <summary>
/// Виконує перевірку команди
/// видалення приватного файлу.
/// </summary>
public class DeletePrivateAttachmentCommandValidator
    : AbstractValidator<DeletePrivateAttachmentCommand>
{
    public DeletePrivateAttachmentCommandValidator()
    {
        RuleFor(command =>
                command.FileId)
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатор файлу повинен бути більшим за нуль.");

        RuleFor(command =>
                command.ParticipantId)
            .ValidParticipantId();
    }
}