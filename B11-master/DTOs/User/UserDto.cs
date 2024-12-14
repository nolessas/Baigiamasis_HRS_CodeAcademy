public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public List<string> Roles { get; set; }
    public bool HasHumanInformation { get; set; }
    public DateTime CreatedDate { get; set; }
} 