namespace SophieHR.Api.Models.DTOs.Department
{
    public class DepartmentDetailDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string Name { get; set; }
        public Guid CompanyId { get; set; }
    }
}