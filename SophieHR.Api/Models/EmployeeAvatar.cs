namespace SophieHR.Api.Models
{
    public class EmployeeAvatar:Base
    {
        public Guid EmployeeId { get; set; }
        public byte Avatar { get; set; }
    }
}
