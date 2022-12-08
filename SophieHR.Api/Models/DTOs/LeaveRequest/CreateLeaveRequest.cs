namespace SophieHR.Api.Models.DTOs.LeaveRequest
{
    public class CreateLeaveRequest
    {
        public Guid EmployeeId { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public bool StartDateFirstHalf { get; set; }
        public bool StartDateSecondHalf { get; set; }
        public bool EndDateFirstHalf { get; set; }
        public bool EndDateSecondHalf { get; set; }
    }
}
