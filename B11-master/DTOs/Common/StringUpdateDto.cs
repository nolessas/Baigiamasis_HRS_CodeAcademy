using System.ComponentModel.DataAnnotations;

public class StringUpdateDto
{
    [Required(ErrorMessage = "Value is required")]
    [MinLength(1, ErrorMessage = "Value cannot be empty")]
    [RegularExpression(@".*\S+.*", ErrorMessage = "Value cannot contain only whitespace")]
    public string Value { get; set; }
} 