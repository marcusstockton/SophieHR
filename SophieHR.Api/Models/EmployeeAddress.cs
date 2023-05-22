namespace SophieHR.Api.Models
{
    public class EmployeeAddress : Address
    {
        //[Obsolete("REQUIRED FOR ENTITY FRAMEWORK - DO NOT USE OR REMOVE")]
        public EmployeeAddress()
        {
            AddressType = AddressType.Employee;
        }

        public EmployeeAddress(AddressType addressType) : base(addressType)
        {
            base.AddressType = AddressType.Employee;
        }
    }
}