namespace SophieHR.Api.Models.DTOs.Company
{
    public class CompanyCreateDto
    {
        public string Name { get; set; }
        public CompanyAddress Address { get; set; }
    }
}
