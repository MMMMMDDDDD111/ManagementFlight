using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightManagement.Models;
using FlightManagement.Models.Management_Flight;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using FlightManagement.Models.Authentication.Login;
using System.ComponentModel;
using FlightManagement.NewFolder;
using static FlightManagement.NewFolder.EnumExtensions;
using X.PagedList;
using static FlightManagement.Controllers.GroupsController;

namespace FlightManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class DocumentInformationsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        private readonly RoleManager<IdentityRole> _roleManager;

        public DocumentInformationsController(ApplicationDBContext context, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

        // GET: api/DocumentInformations
        [HttpGet]
        public async Task<IActionResult> GetDocumentInfo(int? page)
        {
            if (_context.DocumentInfo == null)
            {
                return NotFound();
            }

            var documentInfos = _context.DocumentInfo.Include(d => d.AddFlight).ToList();

            int pageSize = 3; 

            int pageNumber = page ?? 1;

            var pagedDocumentInfos = documentInfos.ToPagedList(pageNumber, pageSize);

            return new JsonResult(new { Data = pagedDocumentInfos });
        }



        // GET: api/DocumentInformations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentInformation>> GetDocumentInformation(int id)
        {
          if (_context.DocumentInfo == null)
          {
              return NotFound();
          }
            var documentInformation = await _context.DocumentInfo.FindAsync(id);

            if (documentInformation == null)
            {
                return NotFound();
            }

            return documentInformation;
        }

        [HttpGet("SearchByGroupNameAndFlight")]
        public async Task<ActionResult<IEnumerable<DocumentInformation>>> SearchDocumentsByGroupNameAndFlight(string groupName, int idFlight)
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                return BadRequest("Group name cannot be empty.");
            }

            if (idFlight <= 0)
            {
                return BadRequest("IdFlight must be a positive integer.");
            }

            // Tìm tài liệu (DocumentInformation) dựa trên tên nhóm (GroupName) và IdFlight tương ứng trong tài liệu
            var documents = await _context.DocumentInfo
                .Where(d => d.Groups != null && d.Groups.GroupName == groupName && d.IdFlight == idFlight)
                .ToListAsync();

            if (documents == null || !documents.Any())
            {
                return NotFound("No documents found for the provided group name and IdFlight.");
            }

            return documents;
        }


        // PUT: api/DocumentInformations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDocumentInformation(int id, DocumentInformation documentInformation)
        {
            if (id != documentInformation.Id)
            {
                return BadRequest();
            }

            _context.Entry(documentInformation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocumentInformationExists(id))
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
  
        [HttpPost]
        public async Task<ActionResult<DocumentInformation>> PostDocumentInformation([FromForm] DocumentInformation documentInformation, IFormFile file)
        {
            if (_context.DocumentInfo == null)
            {
                return Problem("Entity set 'ApplicationDBContext.DocumentInfo' is null.");
            }

            // Kiểm tra xem có tệp tải lên không
            if (file != null && file.Length > 0)
            {
                // Lưu tên tệp và lấy đường dẫn
                var uploadedFileName = await Writefile(file);
                documentInformation.FileName = uploadedFileName;
            }

            // Xóa trường "Id" ra khỏi đối tượng để không hiển thị khi tạo mới
            documentInformation.Id = 0;

            _context.DocumentInfo.AddAsync(documentInformation);
            await _context.SaveChangesAsync();

            // Tìm lại đối tượng DocumentInformation sau khi đã lưu vào cơ sở dữ liệu để có ID mới
            documentInformation = _context.DocumentInfo.Find(documentInformation.Id);

            var documentInfos = _context.DocumentInfo.Include(d => d.AddFlight).ToList();

            if (documentInformation != null)
            {
                // Lấy AddFlight cụ thể dựa trên AddFlightId
                var addFlight = _context.Addflights.Find(documentInformation.Id);

                if (addFlight != null)
                {
                    documentInformation.AddFlight = addFlight;
                    int addFlightId = documentInformation.IdFlight;
                    // Làm việc với AddFlightDTO ở đây nếu cần

                    return CreatedAtAction(nameof(GetDocumentInfo), new { id = documentInformation.Id }, documentInformation);
                }
                else
                {
                    return BadRequest("AddFlight with the specified AddFlightId not found.");
                }
            }
            else
            {
                return BadRequest("DocumentInformation not found.");
            }

        }

        //DOWLOAD FILES
        [HttpGet]
        [Route("DowloadFile")]
        public async Task<IActionResult> DowloadFile(string filename)
        {
            var filepath = Path.Combine(Directory.GetCurrentDirectory(), "Upload\\Files", filename);

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filepath, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            var bytes = await System.IO.File.ReadAllBytesAsync(filepath);
            return File(bytes, contentType, Path.GetFileName(filepath));
        }
        // DELETE: api/DocumentInformations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocumentInformation(int id)
        {
            if (_context.DocumentInfo == null)
            {
                return NotFound();
            }
            var documentInformation = await _context.DocumentInfo.FindAsync(id);
            if (documentInformation == null)
            {
                return NotFound();
            }

            _context.DocumentInfo.Remove(documentInformation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DocumentInformationExists(int id)
        {
            return (_context.DocumentInfo?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
