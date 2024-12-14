namespace Baigiamasis.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public Guid UserId { get; set; }
        public List<string> Roles { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}