namespace HW_06.Validators.ResultsValid;

/// <summary>
/// Представляє результат
/// виконання валідації моделі.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Колекція помилок,
    /// виявлених під час валідації.
    /// </summary>
    public List<ValidationError> Errors { get; } = new();

    /// <summary>
    /// Вказує, чи пройшла
    /// валідація успішно.
    /// </summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Додає нову помилку
    /// до результату валідації.
    /// </summary>
    /// <param name="propertyName">
    /// Назва властивості, для якої виникла помилка.
    /// </param>
    /// <param name="errorMessage">
    /// Текст повідомлення про помилку.
    /// </param>
    public void AddError(
        string propertyName,
        string errorMessage)
    {
        Errors.Add(new ValidationError(propertyName, errorMessage));
    }
}