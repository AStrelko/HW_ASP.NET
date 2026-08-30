using FluentValidation;

namespace HW_06.Features.Participants.Queries.GetAll;

/// <summary>
/// Виконує перевірку параметрів запиту
/// для отримання списку учасників.
/// </summary>
public class GetAllParticipantsQueryValidator
    : AbstractValidator<GetAllParticipantsQuery>
{
    private static readonly string[] AllowedSortFields =
    [
        "id",
        "participantid",
        "firstname",
        "lastname",
        "email",
        "position"
    ];

    /// <summary>
    /// Ініціалізує валідатор запиту
    /// списку учасників.
    /// </summary>
    public GetAllParticipantsQueryValidator()
    {
        RuleFor(query =>
                query.Parameters.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage(
                "Номер сторінки повинен бути більшим або дорівнювати 1.");

        RuleFor(query =>
                query.Parameters.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(
                "Кількість записів на сторінці повинна бути від 1 до 100.");

        RuleFor(query =>
                query.Parameters.Search)
            .MaximumLength(100)
            .WithMessage(
                "Пошуковий рядок не може перевищувати 100 символів.")
            .When(query =>
                !string.IsNullOrWhiteSpace(
                    query.Parameters.Search));

        RuleFor(query =>
                query.Parameters.SortBy)
            .Must(BeAllowedSortField)
            .WithMessage(
                "Недопустиме поле сортування. "
                + "Доступні значення: id, firstname, "
                + "lastname, email, position.")
            .When(query =>
                !string.IsNullOrWhiteSpace(
                    query.Parameters.SortBy));
    }

    /// <summary>
    /// Перевіряє, чи дозволене
    /// поле сортування.
    /// </summary>
    private static bool BeAllowedSortField(
        string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return true;
        }

        var normalizedSortField =
            sortBy.Trim().ToLowerInvariant();

        return AllowedSortFields.Contains(
            normalizedSortField);
    }
}