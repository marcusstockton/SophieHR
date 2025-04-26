namespace SophieHR.Api.Models
{
    public class LeaveRequest : Base
    {
        public Guid EmployeeId { get; set; }
        public Guid? ApprovedById { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public int Hours { get; set; }
        public int NormalHoursPerDay { get; set; }
        public bool Approved { get; set; }
        public string Comments { get; set; }
        public LeaveType LeaveType { get; set; }
    }
}