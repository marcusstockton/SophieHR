using SophieHR.Api.Models.DTOs.Address;

namespace SophieHR.Api.Models.DTOs.Company
{
    public class CompanyDetailNoLogo
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string Name { get; set; }
        public AddressBasic Address { get; set; }
    }
}