using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SophieHR.Api.Data;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Notes;

namespace SophieHR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "CompanyManagement")]
    public class NotesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public readonly IMapper _mapper;

        public NotesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Notes
        [HttpGet("get-notes-for-employee/{employeeId}"), Produces(typeof(IEnumerable<NoteDetailDto>))]
        public async Task<ActionResult<IEnumerable<NoteDetailDto>>> GetNotesForEmployee(Guid employeeId)
        {
            var notes = await _context.Notes.Where(x => x.EmployeeId == employeeId).ToListAsync();

            return Ok(_mapper.Map<IEnumerable<NoteDetailDto>>(notes));
        }

        // GET: api/Notes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NoteDetailDto>> GetNote(Guid id)
        {
            var note = await _context.Notes.FindAsync(id);

            if (note == null)
            {
                return NotFound();
            }

            return _mapper.Map<NoteDetailDto>(note);
        }

        // PUT: api/Notes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNotes(Guid id, NoteDetailDto noteDto)
        {
            if (id != noteDto.Id)
            {
                return BadRequest();
            }
            var note = _mapper.Map<Note>(noteDto);
            _context.Entry(note).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NotesExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Notes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("{employeeId}")]
        public async Task<ActionResult<NoteDetailDto>> PostNotes([FromBody] NoteCreateDto noteInput, [FromRoute] Guid employeeId)
        {
            var note = _mapper.Map<Note>(noteInput);
            note.EmployeeId = employeeId;
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNote), new { id = note.Id }, note);
        }

        // DELETE: api/Notes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotes(Guid id)
        {
            var notes = await _context.Notes.FindAsync(id);
            if (notes == null)
            {
                return NotFound();
            }

            _context.Notes.Remove(notes);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("GetNoteTypes"), Produces(typeof(Dictionary<int, string>))]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
        public ActionResult<Dictionary<int, string>> GetNoteTypes()
        {
            var dict = Enum.GetValues(typeof(NoteType))
               .Cast<NoteType>()
               .ToDictionary(t => (int)t, t => t.ToString());
            return Ok(dict);
        }

        private bool NotesExists(Guid id)
        {
            return _context.Notes.Any(e => e.Id == id);
        }
    }
}