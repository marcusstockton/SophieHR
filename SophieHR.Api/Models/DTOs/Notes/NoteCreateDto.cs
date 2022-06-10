namespace SophieHR.Api.Models.DTOs.Notes
{
    public class NoteCreateDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public NoteType NoteType { get; set; }
    }
}
