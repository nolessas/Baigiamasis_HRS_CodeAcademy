using System.ComponentModel.DataAnnotations;

namespace Baigiamasis.Models
{
    public class HumanInformation
    {
        public Guid Id { get; set; }


        public string FirstName { get; set; }


        public string LastName { get; set; }

      
        public string PersonalCode { get; set; }

        
        public string PhoneNumber { get; set; }

      
        public string Email { get; set; }

   
        public byte[] ProfilePicture { get; set; }

        public Guid UserId { get; set; }

        public User User { get; set; }
        
        public Address Address { get; set; }
    }
} 