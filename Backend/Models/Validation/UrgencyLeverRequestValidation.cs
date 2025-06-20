using System.ComponentModel.DataAnnotations;

namespace ProyectoCaritas.Models.Validation
{
    public class UrgencyLevelValidationAttribute : ValidationAttribute
    {
        private readonly string[] _allowedValues = { "Alto", "Bajo" };

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string stringValue)
            {
                if (_allowedValues.Contains(stringValue, StringComparer.OrdinalIgnoreCase))
                    return ValidationResult.Success;

                return new ValidationResult($"Nivel de urgencia debe ser 'Bajo' o 'Alto'.");
            }

            return new ValidationResult("Nivel de urgencia es requerido.");
        }
    }
}
