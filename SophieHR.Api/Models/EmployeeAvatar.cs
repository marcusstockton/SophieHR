using System.ComponentModel.DataAnnotations;

namespace SophieHR.Api.Models
{
    public class EmployeeAvatar : Base
    {
        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; }

        [Base64String]
        public byte[] Avatar { get; set; }
    }
}