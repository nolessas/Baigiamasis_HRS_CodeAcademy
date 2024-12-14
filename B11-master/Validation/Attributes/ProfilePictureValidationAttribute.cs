using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Baigiamasis.Validation.Attributes
{
    public class ProfilePictureValidationAttribute : ValidationAttribute
    {
        private readonly int _maxSizeInMb;
        private readonly string[] _allowedTypes = { "image/jpeg", "image/png", "image/gif" };

        public ProfilePictureValidationAttribute(int maxSizeInMb = 5)
        {
            _maxSizeInMb = maxSizeInMb;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult("Profile picture is required");

            if (value is not IFormFile file)
                return new ValidationResult("Invalid file format");

            // Check file type
            if (!_allowedTypes.Contains(file.ContentType.ToLower()))
                return new ValidationResult("Invalid file type. Only JPEG, PNG and GIF are allowed.");

            // Check file size
            if (file.Length > _maxSizeInMb * 1024 * 1024)
                return new ValidationResult($"File size too large. Maximum size is {_maxSizeInMb}MB.");

            return ValidationResult.Success;
        }
    }
} 