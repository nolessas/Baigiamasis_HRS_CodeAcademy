public class HumanInformationDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PersonalCode { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string? ProfilePictureBase64 { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
    public string HouseNumber { get; set; }
    public string? ApartmentNumber { get; set; }
    public Guid UserId { get; set; }
} 