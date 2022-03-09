using SophieHR.Api.Models.DTOs.Address;

namespace SophieHR.Api.Models.DTOs.Company
{
    public class CompanyCreateDto
    {
        public string Name { get; set; }
        public AddressCreateDto Address { get; set; }
    }
}