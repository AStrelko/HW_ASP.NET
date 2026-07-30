using HW_06.Validators.ResultsValid;

namespace HW_06.Validators;

/// <summary>
/// Визначає контракт для виконання
/// валідації моделей.
/// </summary>
public interface IValidator<in T>
{
    /// <summary>
    /// Виконує перевірку моделі.
    /// </summary>
    /// <param name="model">
    /// Модель, яку необхідно перевірити.
    /// </param>
    /// <returns>
    /// Результат валідації, що містить інформацію
    /// про успішність перевірки та можливі помилки.
    /// </returns>
    ValidationResult Validate(T model);
}