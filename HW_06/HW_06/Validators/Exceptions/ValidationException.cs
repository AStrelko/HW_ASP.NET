using HW_06.Validators.ResultsValid;

namespace HW_06.Validators.Exceptions;

/// <summary>
/// Виняток, який виникає під час помилки
/// валідації вхідних даних.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Список помилок, виявлених під час валідації.
    /// </summary>
    public List<ValidationError> Errors { get; }

    /// <summary>
    /// Ініціалізує новий екземпляр винятку
    /// на основі списку помилок валідації.
    /// </summary>
    /// <param name="errors">
    /// Колекція помилок валідації.
    /// </param>
    public ValidationException(IEnumerable<ValidationError> errors)
        : base("Перевірка даних не пройдена.")
    {
        Errors = errors.ToList();
    }

    /// <summary>
    /// Ініціалізує новий екземпляр винятку
    /// для однієї помилки валідації.
    /// </summary>
    /// <param name="propertyName">
    /// Назва властивості, для якої виникла помилка.
    /// </param>
    /// <param name="errorMessage">
    /// Текст повідомлення про помилку.
    /// </param>
    public ValidationException(
        string propertyName,
        string errorMessage)
        : base(errorMessage)
    {
        Errors = new List<ValidationError>
        {
            new(propertyName, errorMessage)
        };
    }
}