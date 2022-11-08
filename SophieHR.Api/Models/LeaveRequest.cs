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
        public bool StartDateFirstHalf { get; set; }
        public bool StartDateSecondHalf { get; set; }
        public bool EndDateFirstHalf { get; set; }
        public bool EndDateSecondHalf { get; set; }
        public bool Approved { get; set; }
    }
}
