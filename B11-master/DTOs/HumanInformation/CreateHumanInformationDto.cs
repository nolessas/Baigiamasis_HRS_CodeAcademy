using System.ComponentModel.DataAnnotations;
using Baigiamasis.Validation.Attributes;

namespace Baigiamasis.DTOs.HumanInformation
{
    public class CreateHumanInformationDto
    {
        [Required]
        [StringLength(50)]
        [NoWhitespace]
        [RegularExpression(@"^[A-ZĄČĘĖĮŠŲŪŽ][a-ząčęėįšųūž]+$", 
            ErrorMessage = "First name must start with uppercase and contain only Lithuanian letters")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [NoWhitespace]
        [RegularExpression(@"^[A-ZĄČĘĖĮŠŲŪŽ][a-ząčęėįšųūž]+$", 
            ErrorMessage = "Last name must start with uppercase and contain only Lithuanian letters")]
        public string LastName { get; set; }

        [Required]
        [RegularExpression(@"^[3-6]\d{2}(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])\d{4}$",
            ErrorMessage = "Invalid Lithuanian personal code format (11 digits starting with 3-6)")]
        public string PersonalCode { get; set; }

        [Required]
        [RegularExpression(@"^\+370\d{8}$",
            ErrorMessage = "Phone number must be in format: +370xxxxxxxx")]
        public string PhoneNumber { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@(gmail\.com|yahoo\.com)$",
            ErrorMessage = "Invalid email format (only gmail.com and yahoo.com domains are allowed)")]
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        [NoWhitespace]
        [RegularExpression(@"^[A-ZĄČĘĖĮŠŲŪŽ][a-ząčęėįšųūž\s-]+$",
            ErrorMessage = "City must start with uppercase and contain only letters, spaces and hyphens")]
        public string City { get; set; }

        [Required]
        [StringLength(100)]
        [NoWhitespace]
        [RegularExpression(@"^[A-ZĄČĘĖĮŠŲŪŽ][a-ząčęėįšųūž\s-]+$",
            ErrorMessage = "Street must start with uppercase and contain only letters, spaces and hyphens")]
        public string Street { get; set; }

        [Required]
        [StringLength(10)]
        [NoWhitespace]
        [RegularExpression(@"^[1-9]\d{0,3}[A-Za-z]?$",
            ErrorMessage = "House number must be a number (1-9999) optionally followed by a letter")]
        public string HouseNumber { get; set; }

        [StringLength(10)]
        [RegularExpression(@"^[1-9]\d{0,3}$|^$",
            ErrorMessage = "Apartment number must be a positive number or empty")]
        public string? ApartmentNumber { get; set; }
    }
}