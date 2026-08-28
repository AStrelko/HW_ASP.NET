using FluentValidation;

namespace HW_06.Features.Meetings.Queries.GetByParticipant;

/// <summary>
/// Виконує перевірку запиту
/// на отримання зустрічей учасника.
/// </summary>
public class GetMeetingsByParticipantQueryValidator
    : AbstractValidator<GetMeetingsByParticipantQuery>
{
    public GetMeetingsByParticipantQueryValidator()
    {
        RuleFor(query =>
                query.ParticipantId)
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатор учасника повинен бути більшим за нуль.");
    }
}