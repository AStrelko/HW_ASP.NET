using FluentValidation;
using HW_06.Features.Common.Identity;

namespace HW_06.Features.PrivateAttachments.Queries.GetReceived;

/// <summary>
/// Виконує перевірку запиту
/// на отримання приватних файлів учасника.
/// </summary>
public class GetReceivedPrivateAttachmentsQueryValidator
    : AbstractValidator<GetReceivedPrivateAttachmentsQuery>
{
    public GetReceivedPrivateAttachmentsQueryValidator()
    {
        RuleFor(query =>
                query.ParticipantId)
            .ValidParticipantId();
    }
}