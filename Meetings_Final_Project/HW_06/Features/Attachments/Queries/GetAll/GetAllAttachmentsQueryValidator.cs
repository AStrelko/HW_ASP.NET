using FluentValidation;

namespace HW_06.Features.Attachments.Queries.GetAll;

/// <summary>
/// Виконує перевірку запиту
/// на отримання публічних файлів зустрічі.
/// </summary>
public class GetAllAttachmentsQueryValidator
    : AbstractValidator<GetAllAttachmentsQuery>
{
    public GetAllAttachmentsQueryValidator()
    {
        RuleFor(query =>
                query.MeetingId)
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатор зустрічі повинен бути більшим за нуль.");
    }
}