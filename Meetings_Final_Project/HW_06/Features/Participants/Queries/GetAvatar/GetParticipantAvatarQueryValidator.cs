using FluentValidation;

namespace HW_06.Features.Participants.Queries.GetAvatar;

/// <summary>
/// Виконує перевірку запиту
/// на отримання аватара учасника.
/// </summary>
public class GetParticipantAvatarQueryValidator
    : AbstractValidator<GetParticipantAvatarQuery>
{
    public GetParticipantAvatarQueryValidator()
    {
        RuleFor(query =>
                query.ParticipantId)
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатор учасника повинен бути більшим за нуль.");
    }
}