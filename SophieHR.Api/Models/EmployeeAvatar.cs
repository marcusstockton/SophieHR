namespace SophieHR.Api.Models
{
    public class EmployeeAvatar : Base
    {
        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public byte[] Avatar { get; set; }
    }
}