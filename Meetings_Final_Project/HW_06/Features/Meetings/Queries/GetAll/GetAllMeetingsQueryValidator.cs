using FluentValidation;

namespace HW_06.Features.Meetings.Queries.GetAll;

/// <summary>
/// Виконує перевірку параметрів запиту
/// для отримання списку зустрічей.
/// </summary>
public class GetAllMeetingsQueryValidator
    : AbstractValidator<GetAllMeetingsQuery>
{
    private static readonly string[] AllowedSortFields =
    [
        "id",
        "meetingid",
        "title",
        "date",
        "datetime",
        "room",
        "roomnumber",
        "participants",
        "participantscount"
    ];

    /// <summary>
    /// Ініціалізує валідатор запиту
    /// списку зустрічей.
    /// </summary>
    public GetAllMeetingsQueryValidator()
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
                + "Доступні значення: id, title, date, "
                + "room, participants.")
            .When(query =>
                !string.IsNullOrWhiteSpace(
                    query.Parameters.SortBy));

        RuleFor(query =>
                query.Filter.RoomNumber)
            .GreaterThan(0)
            .WithMessage(
                "Номер кімнати повинен бути більшим за нуль.")
            .When(query =>
                query.Filter.RoomNumber.HasValue);

        RuleFor(query =>
                query.Filter)
            .Must(filter =>
                !filter.StartTime.HasValue ||
                !filter.EndTime.HasValue ||
                filter.StartTime.Value <=
                filter.EndTime.Value)
            .WithMessage(
                "Початкова дата фільтрації "
                + "не може бути пізнішою за кінцеву.");
    }

    /// <summary>
    /// Перевіряє, чи дозволене поле сортування.
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