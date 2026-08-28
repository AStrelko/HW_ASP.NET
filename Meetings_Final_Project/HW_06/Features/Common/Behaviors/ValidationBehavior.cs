using FluentValidation;
using MediatR;

namespace HW_06.Features.Common.Behaviors;

/// <summary>
/// Виконує FluentValidation-перевірку
/// MediatR-запитів перед їх передачею
/// відповідному обробнику.
/// </summary>
/// <typeparam name="TRequest">
/// Тип MediatR-запиту.
/// </typeparam>
/// <typeparam name="TResponse">
/// Тип результату MediatR-запиту.
/// </typeparam>
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>>
        _validators;

    /// <summary>
    /// Ініціалізує компонент конвеєра валідації.
    /// </summary>
    /// <param name="validators">
    /// Валідатори для поточного типу MediatR-запиту.
    /// </param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        ArgumentNullException.ThrowIfNull(validators);

        _validators = validators;
    }

    /// <summary>
    /// Виконує всі валідатори для поточного запиту.
    /// Якщо виявлено помилки, генерує
    /// <see cref="ValidationException"/>.
    /// </summary>
    /// <param name="request">
    /// Поточний MediatR-запит.
    /// </param>
    /// <param name="next">
    /// Наступний компонент конвеєра MediatR.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
    /// <returns>
    /// Результат виконання MediatR-запиту.
    /// </returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(_validators.Select(
                    validator => validator.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
                .SelectMany(result => result.Errors)
                .Where(error => error is not null)
                .ToList();

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}