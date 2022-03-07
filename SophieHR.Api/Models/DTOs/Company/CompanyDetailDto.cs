namespace SophieHR.Api.Models.DTOs.Company
{
    public class CompanyDetailDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string Name { get; set; }
        public string? Logo { get; set; }
        public CompanyAddress Address { get; set; }
    }
}
