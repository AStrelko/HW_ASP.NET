using FluentValidation;

namespace HW_06.Features.Attachments.Commands.Upload;

/// <summary>
/// Виконує перевірку команди
/// завантаження публічного файлу.
/// </summary>
public class UploadAttachmentCommandValidator
    : AbstractValidator<UploadAttachmentCommand>
{
    public UploadAttachmentCommandValidator()
    {
        RuleFor(command =>
                command.MeetingId)
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатор зустрічі повинен бути більшим за нуль.");

        RuleFor(command =>
                command.File)
            .NotNull()
            .WithMessage(
                "Файл є обов’язковим.");
    }
}