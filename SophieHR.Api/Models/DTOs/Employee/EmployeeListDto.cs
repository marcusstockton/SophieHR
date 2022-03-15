namespace SophieHR.Api.Models.DTOs.Employee
{
    public class EmployeeListDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string LastName { get; set; }
        public string WorkEmailAddress { get; set; }
        public string? PersonalEmailAddress { get; set; }
        public string WorkPhoneNumber { get; set; }
        public string? WorkMobileNumber { get; set; }
        public string? PersonalMobileNumber { get; set; }
        public double HolidayAllowance { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime StartOfEmployment { get; set; }
        public EmployeeAddress Address { get; set; }
        //public Employee? Manager { get; set; }
        public Guid DepartmentId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid EmployeeAvatarId { get; set; }
        //public virtual Department Department { get; set; }
        //public virtual Company Company { get; set; }
    }
}
