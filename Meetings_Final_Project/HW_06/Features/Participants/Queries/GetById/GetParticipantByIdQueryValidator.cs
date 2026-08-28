using FluentValidation;

namespace HW_06.Features.Participants.Queries.GetById;

/// <summary>
/// Виконує перевірку запиту
/// на отримання учасника за ідентифікатором.
/// </summary>
public class GetParticipantByIdQueryValidator
    : AbstractValidator<GetParticipantByIdQuery>
{
    public GetParticipantByIdQueryValidator()
    {
        RuleFor(query =>
                query.Id)
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатор учасника повинен бути більшим за нуль.");
    }
}