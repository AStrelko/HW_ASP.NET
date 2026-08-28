using FluentValidation;
using HW_06.Features.Common.Identity;

namespace HW_06.Features.PrivateAttachments.Commands.Upload;

/// <summary>
/// Виконує перевірку команди
/// надсилання приватного файлу.
/// </summary>
public class UploadPrivateAttachmentCommandValidator
    : AbstractValidator<UploadPrivateAttachmentCommand>
{
    public UploadPrivateAttachmentCommandValidator()
    {
        RuleFor(command =>
                command.SenderParticipantId)
            .ValidParticipantId();

        RuleFor(command =>
                command.RecipientParticipantId)
            .ValidParticipantId();

        RuleFor(command =>
                command.RecipientParticipantId)
            .NotEqual(command =>
                command.SenderParticipantId)
            .WithMessage(
                "Учасник не може надіслати приватний файл самому собі.");

        RuleFor(command =>
                command.File)
            .NotNull()
            .WithMessage(
                "Файл є обов’язковим.");
    }
}