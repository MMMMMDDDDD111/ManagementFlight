using FlightManagement.Models.Management_Flight;
using FlightManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace FlightManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddFlightsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly DocumentInformation documentInformation;

        public AddFlightsController(ApplicationDBContext context) => _context = context;

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AddFlight>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAll()
        {
            var addflights = await _context.Addflights
                .AsNoTracking()
                .Include(af => af.DocumentInformation)
                .ToListAsync();

            if (addflights == null)
            {
                return NotFound();
            }

            // Tạo danh sách mới chứa thông tin bạn cần
            var results = addflights.Select(addflight => new
            {
                Flightno = addflight.Flightno,
                Date = addflight.Date,
                Pointofloading = addflight.Pointofloding,
                Pointofunloading = addflight.Pointofunloading,
                DocumentInformation = new
                {
                    DocumentName = addflight.DocumentInformation.Select(di => di.Documentname).FirstOrDefault(),
                    DocumentType = addflight.DocumentInformation.Select(di => di.Documenttype).FirstOrDefault(),
                    DocumentVersion = addflight.DocumentInformation.Select(di => di.Documentversion).FirstOrDefault()
                }
            });

            return Ok(results);
        }

        public class AddFlightDTO
        {
            public string? Documentname { get; set; }
            public string? Documenttype { get; set; }
            public string? Documentversion { get; set; }
            public string? Note { get; set; }
            public string? FileName { get; set; }
            public int Id { get; set; }
            [Required]
            public string? Flightno { get; set; }
            [Required]
            public DateTime? Date { get; set; }
            [Required]
            public string? Pointofloding { get; set; }
            [Required]
            public string? Pointofunloading { get; set; }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<AddFlightDTO>> Create([FromForm] AddFlight addflight, IFormFile file)
        {
            if (addflight == null)
            {
                return BadRequest("Invalid data in the request.");
            }


            // Lấy Id của AddFlight sau khi đã thêm vào cơ sở dữ liệu
            int addFlightId = addflight.FlightId;
            DocumentInformation firstDocument = null;
            if (addflight.DocumentInformation != null && addflight.DocumentInformation.Any())
            {
                firstDocument = addflight.DocumentInformation.First();
            }


            // Tạo đối tượng AddFlightDTO từ addFlight
            var addFlightDTO = new AddFlightDTO
            {
                Id = addflight.FlightId,
                Flightno = addflight.Flightno,
                Date = addflight.Date,
                Pointofloding = addflight.Pointofloding,
                Pointofunloading = addflight.Pointofunloading,
                Documentname = addflight.DocumentInformation.Select(di => di.Documentname).FirstOrDefault(),
                Documenttype = addflight.DocumentInformation.Select(di => di.Documenttype).FirstOrDefault(),
                Documentversion = addflight.DocumentInformation.Select(di => di.Documentversion).FirstOrDefault(),
                Note = addflight.DocumentInformation.Select(di => di.Note).FirstOrDefault(),
                FileName = addflight.DocumentInformation.Select(di => di.FileName).FirstOrDefault()
            };
           
            // Thêm AddFlight vào cơ sở dữ liệu
            await _context.Addflights.AddAsync(addflight);
            await _context.SaveChangesAsync();

            if (addflight.DocumentInformation != null)
            {
                _context.DocumentInfo.AddRange(addflight.DocumentInformation);
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetAll), new { id = addFlightId }, addFlightDTO);
        }


        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id,AddFlight addflight)
        {
            if (id != addflight.FlightId) return BadRequest();
            _context.Entry(addflight).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            var documentinfo = await _context.DocumentInfo.Where(w => w.Id == addflight.FlightId).ToArrayAsync();
            if (documentinfo.Count() > 0)
            {
                _context.DocumentInfo.RemoveRange(documentinfo);
                await _context.SaveChangesAsync();
            }
            if (addflight.DocumentInformation != null)
            {
                _context.DocumentInfo.AddRange(addflight.DocumentInformation);
                await _context.SaveChangesAsync();
            }
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
