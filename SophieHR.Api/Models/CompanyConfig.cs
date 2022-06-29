namespace SophieHR.Api.Models
{
    public class CompanyConfig:Base
    {
        public Guid CompanyId { get; set; }
        public virtual Company Company { get; set; }
        public int GdprRetentionPeriodInYears { get; set; }
        
    }
}
