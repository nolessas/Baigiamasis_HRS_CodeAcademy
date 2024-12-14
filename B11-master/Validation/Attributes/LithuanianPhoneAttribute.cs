using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Baigiamasis.Validation.Attributes
{
    public class LithuanianPhoneAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult("Phone number is required");

            string phone = value.ToString();
            
            // Format: +370XXXXXXXX or 8XXXXXXXX
            if (!Regex.IsMatch(phone, @"^(\+370|8)\d{8}$"))
                return new ValidationResult("Invalid Lithuanian phone number format. Use +370XXXXXXXX or 8XXXXXXXX");

            return ValidationResult.Success;
        }
    }
} 