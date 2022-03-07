namespace SophieHR.Api.Models
{
    public class CompanyAddress : Address
    {
        [Obsolete("REQUIRED FOR ENTITY FRAMEWORK - DO NOT USE OR REMOVE")]
        public CompanyAddress()
        {
        }

        public CompanyAddress(AddressType addressType) : base(addressType)
        {
            base.AddressType = AddressType.Company;
        }
    }
}
