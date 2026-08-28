using FluentValidation;

namespace HW_06.Features.Attachments.Queries.Download;

/// <summary>
/// Виконує перевірку запиту
/// на завантаження публічного файлу.
/// </summary>
public class DownloadAttachmentQueryValidator
    : AbstractValidator<DownloadAttachmentQuery>
{
    public DownloadAttachmentQueryValidator()
    {
        RuleFor(query =>
                query.MeetingId)
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатор зустрічі повинен бути більшим за нуль.");

        RuleFor(query =>
                query.AttachmentId)
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатор файлу повинен бути більшим за нуль.");
    }
}