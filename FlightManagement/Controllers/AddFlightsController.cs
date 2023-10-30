using FlightManagement.Models.Management_Flight;
using FlightManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlightManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddFlightsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public AddFlightsController(ApplicationDBContext context) => _context = context;

        [HttpGet]
        public async Task<IEnumerable<AddFlight>> Get()
            => await _context.Addflights.ToListAsync();
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AddFlight), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetbyId(int id)
        {
            var addflight = await _context.Addflights.FindAsync(id);
            return addflight == null ? NotFound() : Ok(addflight);
        }
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(AddFlight addflight)
        {
            // Không cần gán giá trị cho trường Id
            await _context.Addflights.AddAsync(addflight);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetbyId), new { id = addflight.Id }, addflight);
        }
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id,AddFlight addflight)
        {
            if (id != addflight.Id) return BadRequest();
            _context.Entry(addflight).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var addflyToDelete = await _context.Addflights.FindAsync(id);
            if(addflyToDelete == null) return NotFound();
            
            _context.Addflights.Remove(addflyToDelete);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
