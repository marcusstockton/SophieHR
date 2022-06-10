namespace SophieHR.Api.Models.DTOs.Notes
{
    public class NoteDetailDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public Guid EmployeeId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public NoteType NoteType { get; set; }
    }
}
