namespace SophieHR.Api.Models.DTOs.Department
{
    public class DepartmentCreateDto
    {
        public string Name { get; set; }
        public Guid CompanyId { get; set; }
    }
}