using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Baigiamasis.Validation.Attributes
{
    public class PersonalCodeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult("Personal code is required");

            string personalCode = value.ToString();
            
            // Lithuanian personal code format: YYMMDDNNNNN (11 digits)
            if (!Regex.IsMatch(personalCode, @"^\d{11}$"))
                return new ValidationResult("Invalid personal code format. Must be 11 digits.");

            return ValidationResult.Success;
        }
    }
} 