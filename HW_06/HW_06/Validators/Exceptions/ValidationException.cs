using HW_06.Validators.ResultsValid;

namespace HW_06.Validators.Exceptions;

/// <summary>
/// Исключение, возникающее при ошибке проверки входных данных.
/// </summary>
public class ValidationException : Exception
{
    public List<ValidationError> Errors { get; }

    public ValidationException(IEnumerable<ValidationError> errors)
        : base("Перевірка даних не пройдена.")
    {
        Errors = errors.ToList();
    }

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