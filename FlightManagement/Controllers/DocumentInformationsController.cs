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
        public async Task<ActionResult<IEnumerable<DocumentInformation>>> GetDocumentInfo()
        {
          if (_context.DocumentInfo == null)
          {
              return NotFound();
          }
            var documentsWithRoles = await _context.DocumentInfo.Include(d => d.Role).ToListAsync();

            return documentsWithRoles;
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

            // Kiểm tra xem Role có tồn tại không
            var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.Id == documentInformation.RoleId);

            if (existingRole == null)
            {
                return BadRequest("Role not found.");
            }

            // Gán Role đã tìm thấy cho DocumentInformation
            documentInformation.Role = existingRole;

            // Kiểm tra xem có tệp tải lên không
            if (file != null && file.Length > 0)
            {
                // Lưu tên tệp và lấy đường dẫn
                var uploadedFileName = await Writefile(file);
                documentInformation.FileName = uploadedFileName;
            }

            _context.DocumentInfo.Add(documentInformation);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDocumentInformation", new { id = documentInformation.Id }, documentInformation);
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
