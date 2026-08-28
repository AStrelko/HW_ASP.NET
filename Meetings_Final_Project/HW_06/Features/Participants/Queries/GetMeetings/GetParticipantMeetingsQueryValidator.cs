using FluentValidation;

namespace HW_06.Features.Participants.Queries.GetMeetings;

/// <summary>
/// Виконує перевірку запиту
/// на отримання зустрічей учасника.
/// </summary>
public class GetParticipantMeetingsQueryValidator
    : AbstractValidator<GetParticipantMeetingsQuery>
{
    public GetParticipantMeetingsQueryValidator()
    {
        RuleFor(query =>
                query.ParticipantId)
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатор учасника повинен бути більшим за нуль.");
    }
}