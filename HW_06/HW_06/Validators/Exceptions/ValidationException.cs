namespace HW_06.Validators.Exceptions;

/// <summary>
/// Виняток, який виникає під час
/// порушення бізнес-правил валідації.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Назва властивості,
    /// з якою пов'язана помилка.
    /// </summary>
    public string PropertyName { get; }

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
        PropertyName = propertyName;
    }
}