﻿using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Company;
using SophieHR.Api.Models.DTOs.Department;

namespace SophieHR.Api.Models.DTOs.Employee
{
    public class EmployeeDetailDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
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
        public Guid? ManagerId { get; set; }
        public Guid DepartmentId { get; set; }
        public Guid CompanyId { get; set; }
        public string? AvatarBase64 { get; set; }
        public DepartmentDetailDto Department { get; set; }
        public CompanyDetailDto Company { get; set; }
    }
}