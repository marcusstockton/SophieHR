namespace SophieHR.Api.Models
{
    public enum AddressType
    {
        Company,
        Employee,
    }

    public abstract class Address : Base
    {
        public string Line1 { get; set; }
        public string? Line2 { get; set; }
        public string? Line3 { get; set; }
        public string? Line4 { get; set; }
        public string Postcode { get; set; }
        public string County { get; set; }
        public AddressType AddressType { get; set; }

        //[Obsolete("REQUIRED FOR ENTITY FRAMEWORK - DO NOT USE OR REMOVE")]
        public Address()
        {
        }

        protected Address(AddressType addressType)
        {
            AddressType = addressType;
        }
    }
}