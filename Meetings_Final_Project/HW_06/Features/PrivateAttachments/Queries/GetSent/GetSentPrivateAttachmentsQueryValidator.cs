using FluentValidation;
using HW_06.Features.Common.Identity;

namespace HW_06.Features.PrivateAttachments.Queries.GetSent;

/// <summary>
/// Виконує перевірку запиту
/// на отримання надісланих приватних файлів.
/// </summary>
public class GetSentPrivateAttachmentsQueryValidator
    : AbstractValidator<GetSentPrivateAttachmentsQuery>
{
    public GetSentPrivateAttachmentsQueryValidator()
    {
        RuleFor(query =>
                query.ParticipantId)
            .ValidParticipantId();
    }
}