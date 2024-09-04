using System.ComponentModel.DataAnnotations;

namespace SophieHR.Api.Models.DTOs.Employee.EmployeeAvatar
{
    public class EmployeeAvatarDetail
    {
        public Guid Id { get; set; }

        [Base64String]
        public string? Avatar { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}