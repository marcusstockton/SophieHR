using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SophieHR.Api.Data;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.LeaveRequest;

namespace SophieHR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public readonly IMapper _mapper;

        public LeaveRequestsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("GetLeaveTypes")]
        public ActionResult<Dictionary<int, string>> GetLeaveTypes()
        {
            var dict = new Dictionary<int, string>();
            foreach (var name in Enum.GetNames(typeof(LeaveType)))
            {
                dict.Add((int)Enum.Parse(typeof(LeaveType), name), name);
            }
            return Ok(dict);
        }

        // GET: api/LeaveRequests
        [HttpGet("GetLeaveRequestsForEmployee/{employeeId}")]
        public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetLeaveRequestsForEmployee(Guid employeeId)
        {
            var results = await _context.LeaveRequests.Where(x => x.EmployeeId == employeeId).ToListAsync();
            return Ok(results);
        }

        // GET: api/LeaveRequests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LeaveRequest>> GetLeaveRequest(Guid id)
        {
            var leaveRequest = await _context.LeaveRequests.FindAsync(id);

            if (leaveRequest == null)
            {
                return NotFound();
            }

            return leaveRequest;
        }

        // PUT: api/LeaveRequests/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutLeaveRequest(Guid id, LeaveRequest leaveRequest)
        {
            if (id != leaveRequest.Id)
            {
                return BadRequest();
            }

            _context.Entry(leaveRequest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LeaveRequestExists(id))
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

        // POST: api/LeaveRequests
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<LeaveRequest>> PostLeaveRequest(CreateLeaveRequest leaveRequestDto)
        {
            var leaveRequest = _mapper.Map<LeaveRequest>(leaveRequestDto);

            var existingLeave = _context.LeaveRequests.Where(x => x.EmployeeId == leaveRequest.EmployeeId && (x.StartDate == leaveRequestDto.StartDate || x.EndDate == leaveRequestDto.EndDate)).AsEnumerable();
            if (existingLeave.Any())
            {
                return BadRequest("A Leave Request already exists between these dates.");
            }
            _context.LeaveRequests.Add(leaveRequest);

            //TODO: calcuate the time to take off of leave allowance:
            // Things to consider...
            /*
             * Company year? - Jan - Jan? Financial Year?
             * Don't allow employees to book past the current period
             */
            TimeSpan difference = leaveRequestDto.EndDate - leaveRequestDto.StartDate;
           
            await _context.SaveChangesAsync();

            return Created();
        }

        // DELETE: api/LeaveRequests/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteLeaveRequest(Guid id)
        {
            var leaveRequest = await _context.LeaveRequests.FindAsync(id);
            if (leaveRequest == null)
            {
                return NotFound();
            }

            _context.LeaveRequests.Remove(leaveRequest);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LeaveRequestExists(Guid id)
        {
            return _context.LeaveRequests.Any(e => e.Id == id);
        }
    }
}