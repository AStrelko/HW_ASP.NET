using HW_06.Validators.ResultsValid;

namespace HW_06.Validators;

public interface IValidator<in T>
{
    ValidationResult Validate(T model);
}