namespace HW_06.Validators.ResultsValid;

public class ValidationResult
{
    public List<ValidationError> Errors { get; } = new();
    public bool IsValid => Errors.Count == 0;

    public void AddError(string propertyName, string errorMessage)
    {
        Errors.Add(new ValidationError(propertyName, errorMessage));
    }
}