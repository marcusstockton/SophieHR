using System.ComponentModel.DataAnnotations;

namespace SophieHR.Api.Models.DTOs.LeaveRequest
{
    public class CreateLeaveRequest
    {
        public Guid EmployeeId { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public int Hours { get; set; }
        public int NormalHoursPerDay { get; set; }
        public LeaveType LeaveType { get; set; }

        [MaxLength(250)]
        public string Comments { get; set; }
    }
}