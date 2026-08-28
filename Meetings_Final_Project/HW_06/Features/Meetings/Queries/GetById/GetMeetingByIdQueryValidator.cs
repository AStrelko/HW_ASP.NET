using FluentValidation;

namespace HW_06.Features.Meetings.Queries.GetById;

/// <summary>
/// Виконує перевірку запиту
/// на отримання зустрічі за ідентифікатором.
/// </summary>
public class GetMeetingByIdQueryValidator
    : AbstractValidator<GetMeetingByIdQuery>
{
    public GetMeetingByIdQueryValidator()
    {
        RuleFor(query =>
                query.Id)
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатор зустрічі повинен бути більшим за нуль.");
    }
}