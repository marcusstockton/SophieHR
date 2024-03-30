using SophieHR.Api.Models.DTOs.Address;
using System.ComponentModel.DataAnnotations;

namespace SophieHR.Api.Models.DTOs.Employee
{
    public class EmployeeCreateDto
    {
        public string Username { get; set; }
        public string Title { get; set; }
        public string Gender { get; set; }

        [MaxLength(50)]
        public string FirstName { get; set; }

        public string? MiddleName { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }

        [Required, MaxLength(100), DataType(DataType.EmailAddress)]
        public string WorkEmailAddress { get; set; }

        [MaxLength(100), DataType(DataType.EmailAddress)]
        public string? PersonalEmailAddress { get; set; }

        [MaxLength(50), DataType(DataType.PhoneNumber)]
        public string WorkPhoneNumber { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string? WorkMobileNumber { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string? PersonalMobileNumber { get; set; }

        public double HolidayAllowance { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime StartOfEmployment { get; set; }

        public AddressCreateDto Address { get; set; }
        public Guid? DepartmentId { get; set; }

        [Required]
        public Guid CompanyId { get; set; }

        [MaxLength(9), RegularExpression("^[A-Z0-9<]{9}[0-9]{1}[A-Z]{3}[0-9]{7}[A-Z]{1}[0-9]{7}[A-Z0-9<]{14}[0-9]{2}$", ErrorMessage = "Passport number invalid.")]
        public string? PassportNumber { get; set; }

        [MaxLength(9), RegularExpression("^[A-Za-z]{2}[0-9]{6}[A-Za-z]{1}$", ErrorMessage = "Invalid Nino.")]
        public string? NationalInsuranceNumber { get; set; }
        public string? ManagerId { get; set; }
        public string JobTitle { get; set; }
    }
}