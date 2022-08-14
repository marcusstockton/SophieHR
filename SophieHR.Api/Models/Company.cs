namespace SophieHR.Api.Models
{
    public class Company : Base
    {
        public string Name { get; set; }
        public byte[]? Logo { get; set; }
        public string Postcode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public CompanyAddress Address { get; set; }
        public ICollection<Employee> Employees { get; set; }
        public CompanyConfig CompanyConfig { get; set; }
    }
}