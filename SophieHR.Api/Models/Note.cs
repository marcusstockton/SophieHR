namespace SophieHR.Api.Models
{
    public enum NoteType
    {
        General,
        Leaving,
        Appraisal
    }

    public class Note : Base
    {
        public Guid EmployeeId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public NoteType NoteType { get; set; }
    }
}