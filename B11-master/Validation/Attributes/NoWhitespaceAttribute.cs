using System.ComponentModel.DataAnnotations;

namespace Baigiamasis.Validation.Attributes
{
    public class NoWhitespaceAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            string stringValue = value.ToString();
            
            if (string.IsNullOrWhiteSpace(stringValue))
                return new ValidationResult($"{validationContext.DisplayName} cannot be empty or whitespace");

            return ValidationResult.Success;
        }
    }
} 