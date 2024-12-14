using System.ComponentModel.DataAnnotations;

namespace Baigiamasis.Models
{
    public class Address
    {
        public Guid Id { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string HouseNumber { get; set; }
        public string? ApartmentNumber { get; set; }
        public Guid HumanInformationId { get; set; }
        public HumanInformation HumanInformation { get; set; }
    }
} 