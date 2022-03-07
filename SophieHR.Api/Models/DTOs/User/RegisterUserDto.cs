using System.ComponentModel.DataAnnotations;

namespace SophieHR.Api.Models
{
    public class RegisterUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [DataType(DataType.EmailAddress), Required]
        public string EmailAddress { get; set; }
        [DataType(DataType.Password), Required]
        public string Password { get; set; }
    }
}
