using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

public class CustomEmailDomainAttribute : ValidationAttribute
{
    public string[] AllowedDomains { get; set; }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null) return ValidationResult.Success;

        var email = value.ToString();
        var domain = email.Split('@').LastOrDefault();

        if (domain == null || !AllowedDomains.Contains(domain.ToLower()))
        {
            return new ValidationResult($"Email domain must be one of: {string.Join(", ", AllowedDomains)}");
        }

        return ValidationResult.Success;
    }
} 