using System.ComponentModel.DataAnnotations;

namespace SophieHR.Api.Models
{
    public abstract class Base
    {
        [Key]
        public Guid Id { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}