using FluentValidation;
using HW_06.Features.Common.Identity;

namespace HW_06.Features.PrivateAttachments.Queries.GetById;

/// <summary>
/// Виконує перевірку запиту
/// на отримання приватного файлу.
/// </summary>
public class GetPrivateAttachmentByIdQueryValidator
    : AbstractValidator<GetPrivateAttachmentByIdQuery>
{
    public GetPrivateAttachmentByIdQueryValidator()
    {
        RuleFor(query =>
                query.FileId)
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатор файлу повинен бути більшим за нуль.");

        RuleFor(query =>
                query.ParticipantId)
            .ValidParticipantId();
    }
}