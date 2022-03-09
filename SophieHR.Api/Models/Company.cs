namespace SophieHR.Api.Models
{
    public class Company : Base
    {
        public string Name { get; set; }
        public byte[]? Logo { get; set; }
        public CompanyAddress Address { get; set; }
        public ICollection<Employee> Employees { get; set; }
    }
}