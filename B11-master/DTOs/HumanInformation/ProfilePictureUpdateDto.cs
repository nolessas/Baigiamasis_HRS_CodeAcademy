using Microsoft.AspNetCore.Http;
using Baigiamasis.Validation.Attributes;

namespace Baigiamasis.DTOs.HumanInformation
{
    public class ProfilePictureUpdateDto
    {
        [ProfilePictureValidation]
        public IFormFile ProfilePicture { get; set; }
    }
} 