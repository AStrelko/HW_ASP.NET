using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HW_06.Filters;

/// <summary>
/// Універсальний фільтр для перевірки DTO
/// за допомогою FluentValidation.
/// </summary>
/// <typeparam name="T">
/// Тип DTO, який необхідно перевірити.
/// </typeparam>
public class ValidationFilter<T> : IAsyncActionFilter
{
    private readonly IValidator<T> _validator;

    /// <summary>
    /// Ініціалізує фільтр валідації.
    /// </summary>
    /// <param name="validator">
    /// Валідатор для заданого типу DTO.
    /// </param>
    public ValidationFilter(IValidator<T> validator)
    {
        _validator = validator;
    }

    /// <summary>
    /// Виконує валідацію DTO перед викликом методу контролера.
    /// </summary>
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var argument = context.ActionArguments
            .Values
            .OfType<T>()
            .FirstOrDefault();

        if (argument is null)
        {
            await next();
            return;
        }

        var validationResult =
            await _validator.ValidateAsync(
                argument,
                context.HttpContext.RequestAborted);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(error => error.ErrorMessage)
                        .ToArray());

            context.Result =
                new BadRequestObjectResult(new
                {
                    Message = "Помилка валідації.",
                    Errors = errors
                });

            return;
        }

        await next();
    }
}