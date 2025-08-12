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

        public NotesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Notes
        [HttpGet("get-notes-for-employee/{employeeId}"), Produces(typeof(IEnumerable<NoteDetailDto>))]
        public async Task<ActionResult<IEnumerable<NoteDetailDto>>> GetNotesForEmployee(Guid employeeId)
        {
            var notes = await _context.Notes.Where(x => x.EmployeeId == employeeId).ToListAsync();
            if(notes == null || !notes.Any())
            {
                return new List<NoteDetailDto>();
            }
            var results = notes.Select(n => new NoteDetailDto
            {
                Id = n.Id,
                EmployeeId = n.EmployeeId,
                NoteType = n.NoteType,
                Content = n.Content,
                CreatedDate = n.CreatedDate,
                Title = n.Title,
                UpdatedDate = n.UpdatedDate,
            }).ToList();
            return Ok(results);
        }

        // GET: api/Notes/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(NoteDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetNote(Guid id)
        {
            var note = await _context.Notes.FindAsync(id);

            if (note == null)
            {
                //return NotFound();
                return Problem(statusCode: StatusCodes.Status404NotFound);
            }
            var result = new NoteDetailDto
            {
                Id = note.Id,
                EmployeeId = note.EmployeeId,
                NoteType = note.NoteType,
                Content = note.Content,
                CreatedDate = note.CreatedDate,
                Title = note.Title,
                UpdatedDate = note.UpdatedDate,
            };
            return Ok(result);
        }

        // PUT: api/Notes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutNotes(Guid id, NoteDetailDto noteDto)
        {
            if (id != noteDto.Id)
            {
                //return BadRequest();
                return Problem(statusCode: StatusCodes.Status404NotFound);

            }

            var note = new Note
            {
                Id = noteDto.Id,
                EmployeeId = noteDto.EmployeeId,
                NoteType = noteDto.NoteType,
                Content = noteDto.Content,
                CreatedDate = noteDto.CreatedDate,
                Title = noteDto.Title,
                UpdatedDate = DateTime.UtcNow // Update the updated date to now
            };

            _context.Entry(note).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NotesExists(id))
                {
                    //return NotFound();
                    return Problem(statusCode: StatusCodes.Status404NotFound);

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
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<NoteDetailDto>> PostNotes([FromBody] NoteCreateDto noteInput, [FromRoute] Guid employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if(employee is null)
            {
                //return BadRequest("Unable to find employee");
                return Problem(detail: "Unable to find employee", statusCode: StatusCodes.Status400BadRequest);

            }

            var note = new Note
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                NoteType = noteInput.NoteType,
                Content = noteInput.Content,
                CreatedDate = DateTime.UtcNow,
                Title = noteInput.Title,
                UpdatedDate = DateTime.UtcNow // Set the updated date to now
            };
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
                //return NotFound();
                return Problem(statusCode: StatusCodes.Status404NotFound);
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