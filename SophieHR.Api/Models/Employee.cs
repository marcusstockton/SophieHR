using System.ComponentModel.DataAnnotations;

namespace SophieHR.Api.Models
{
    public class Employee : ApplicationUser
    {
        public Title Title { get; set; }
        public Gender Gender { get; set; }
        public string? MiddleName { get; set; }
        public string WorkEmailAddress { get; set; }
        public string? PersonalEmailAddress { get; set; }
        public string WorkPhoneNumber { get; set; }
        public string? WorkMobileNumber { get; set; }
        public string? PersonalMobileNumber { get; set; }
        public string JobTitle { get; set; }
        public double HolidayAllowance { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime StartOfEmployment { get; set; }
        public EmployeeAddress Address { get; set; }
        public Employee? Manager { get; set; }

        [RegularExpression(@"^[0-9]{10}GBR[0-9]{7}[U,M,F]{1}[0-9]{9}$")]
        public string PassportNumber { get; set; }

        [RegularExpression(@"^\s*[a-zA-Z]{2}(?:\s*\d\s*){6}[a-zA-Z]?\s*$")]
        public string NationalInsuranceNumber { get; set; }

        public Guid DepartmentId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid EmployeeAvatarId { get; set; }
        public virtual Department Department { get; set; }
        public virtual Company Company { get; set; }
        public virtual EmployeeAvatar Avatar { get; set; }
    }

    public enum Title
    {
        Mr,
        Mrs,
        Miss,
        Ms,
        Mx,
        Sir,
        Dr,
        Cllr,
        Lady,
        Lord
    }

    public enum Gender
    {
        Male,
        Female,
        Other
    }
}