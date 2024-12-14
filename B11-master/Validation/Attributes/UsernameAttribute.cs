using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Baigiamasis.Validation.Attributes
{
    public class UsernameAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult("Username is required");

            string username = value.ToString();
            
            if (string.IsNullOrWhiteSpace(username))
                return new ValidationResult("Username cannot be empty");

            if (username.Length < 3 || username.Length > 50)
                return new ValidationResult("Username must be between 3 and 50 characters");

            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9._-]+$"))
                return new ValidationResult("Username can only contain letters, numbers, and .-_");

            return ValidationResult.Success;
        }
    }
} 