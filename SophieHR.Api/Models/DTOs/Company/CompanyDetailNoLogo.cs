namespace SophieHR.Api.Models.DTOs.Company
{
    public class CompanyDetailNoLogo
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string Name { get; set; }
        public string Postcode { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public CompanyAddress Address { get; set; }
    }
}