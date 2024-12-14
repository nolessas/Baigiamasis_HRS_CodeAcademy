using System.ComponentModel.DataAnnotations;

namespace Baigiamasis.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public string Username { get; set; }

        public byte[] PasswordHash { get; set; }

        public byte[] PasswordSalt { get; set; }

        public List<string> Roles { get; set; } = new();

        public DateTime CreatedDate { get; set; }

        public DateTime? LastLogin { get; set; }

        public HumanInformation HumanInformation { get; set; }
    }
}
