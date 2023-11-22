using FlightManagement.Models.Management_Flight;
using FlightManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis;
using X.PagedList;

namespace FlightManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddFlightsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly DocumentInformation documentInformation;
        private readonly ILogger<DocumentInformationsController> _logger;

        public AddFlightsController(ApplicationDBContext context) => _context = context;

        [HttpGet("GetFlightDetails")]
        [ProducesResponseType(typeof(IEnumerable<AddFlightDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFlightDetails([FromQuery] int? page)
        {
            int pageSize = 3;
            int pageNumber = page ?? 1;

            var pagedAddFlights = await _context.Addflights
                .AsNoTracking()
                .Include(af => af.DocumentInformation)
                .OrderByDescending(af => af.FlightId)
                .ToPagedListAsync(pageNumber, pageSize);

            var result = pagedAddFlights.Select(af => new AddFlightDTO
            {
                FlightId = af.FlightId,
                FlightNo = af.Flightno,
                Date = af.Date,
                Route = $"{af.Pointofloding} - {af.Pointofunloading}",
                TotalDocument = af.DocumentInformation.Count,
                Documents = af.DocumentInformation
                    .Join(
                        _context.Group,
                        di => di.GroupId,
                        group => group.GroupId,
                        (di, group) => new DocumentDTO
                        {
                            DocumentName = di.Documentname,
                            DocumentType = di.Documenttype,
                            Creator = group.Creator,
                            CreateDate = af.Date,
                            DocumentVersion = di.Documentversion
                        })
                    .ToList()
            }).ToList();

            return new JsonResult(result);
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<FlightDTO>> Create([FromForm] FlightDTO addFlightDTO)
        {
            if (addFlightDTO == null)
            {
                return BadRequest("Invalid data in the request.");
            }

            var addFlight = new AddFlight
            {
                Flightno = addFlightDTO.Flightno,
                Date = addFlightDTO.Date,
                Pointofloding = addFlightDTO.Pointofloding,
                Pointofunloading = addFlightDTO.Pointofunloading,
                DocumentInformation = new List<DocumentInformation>()
            };

            _context.Addflights.Add(addFlight);
            await _context.SaveChangesAsync();

            int addFlightId = addFlight.FlightId;

            return CreatedAtAction(nameof(GetFlightDetails), new { flightId = addFlightId }, addFlightDTO);
        }

        private async Task<string> Writefile(IFormFile file)
        {
            string filename = "";
            try
            {
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                filename = DateTime.Now.Ticks.ToString() + extension;

                var filepath = Path.Combine(Directory.GetCurrentDirectory(), "Upload\\Files");

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }
                var exactpath = Path.Combine(Directory.GetCurrentDirectory(), "Upload\\Files", filename);
                using (var stream = new FileStream(exactpath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
            }
            return filename;
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromForm] AddFlight addflight)
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
