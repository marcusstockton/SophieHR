namespace SophieHR.Api.Models
{
    public class CompanyAddress : Address
    {
        //[Obsolete("REQUIRED FOR ENTITY FRAMEWORK - DO NOT USE OR REMOVE")]
        public string? MapImage { get; set; }

        public CompanyAddress()
        {
            AddressType = AddressType.Company;
        }

        public CompanyAddress(AddressType addressType) : base(addressType)
        {
            base.AddressType = AddressType.Company;
        }
    }
}