using FluentValidation;

namespace HW_06.Features.Participants.Queries.GetIdByUserId;

/// <summary>
/// Виконує перевірку запиту
/// на отримання ідентифікатора учасника
/// за ідентифікатором користувача Identity.
/// </summary>
public class GetParticipantIdByUserIdQueryValidator
    : AbstractValidator<GetParticipantIdByUserIdQuery>
{
    public GetParticipantIdByUserIdQueryValidator()
    {
        RuleFor(query =>
                query.ApplicationUserId)
            .NotEmpty()
            .WithMessage(
                "Ідентифікатор користувача є обов’язковим.");
    }
}