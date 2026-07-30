namespace HW_06.Validators.ResultsValid;

/// <summary>
/// Представляє інформацію
/// про окрему помилку валідації.
/// </summary>
public class ValidationError
{
    /// <summary>
    /// Назва властивості,
    /// для якої виникла помилка.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Повідомлення
    /// про помилку валідації.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Ініціалізує новий екземпляр
    /// класу <see cref="ValidationError"/>.
    /// </summary>
    /// <param name="propertyName">
    /// Назва властивості, для якої виникла помилка.
    /// </param>
    /// <param name="errorMessage">
    /// Текст повідомлення про помилку.
    /// </param>
    public ValidationError(
        string propertyName,
        string errorMessage)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
    }
}