using System.ComponentModel.DataAnnotations;

namespace SophieHR.Api.Models
{
    public class Department : Base
    {
        public string Name { get; set; }
        public Guid CompanyId { get; set; }
        public Company Company { get; set; }
    }
}