using Microsoft.EntityFrameworkCore;
using SophieHR.Api.Data;
namespace SophieHR.Api.Models
{
    public class LeaveRequest : Base
    {
        public Guid EmployeeId { get; set; }
        public Guid? ApprovedById { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public bool FirstHalf { get; set; }
        public bool SecondHalf { get; set; }
        public bool Approved { get; set; }
    }
}
