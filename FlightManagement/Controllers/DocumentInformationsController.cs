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
using Elasticsearch.Net;

namespace FlightManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class DocumentInformationsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<DocumentInformationsController> _logger;

        public DocumentInformationsController(ApplicationDBContext context, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

        // GET: api/DocumentInformations
        [HttpGet("Docs-List")]
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

            // Chuyển đổi danh sách DocumentInformation thành danh sách DocsList
            var docsList = new List<DocsList>();

            foreach (var documentInfo in pagedDocumentInfos)
            {
                var group = _context.Group.Find(documentInfo.GroupId);
                var docsListItem = new DocsList
                {
                    DocumentName = documentInfo.Documentname,
                    Type = documentInfo.Documenttype,
                    CreateDate = documentInfo.UpdateDate ?? DateTime.MinValue,
                    FlightNO = documentInfo.AddFlight?.Flightno ?? string.Empty
                };

                // Kiểm tra xem documentInfo có GroupId và Group tồn tại không
                if (group != null)
                {
                    docsListItem.Creator = group.Creator;
                }

                docsList.Add(docsListItem);
            }

            return new JsonResult(new { Data = docsList });
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
        public async Task<ActionResult<DocumentInformation>> PostDocumentInformation([FromForm] DocumentInfoDTO requestDTO, IFormFile file)
        {
            if (_context.DocumentInfo == null)
            {
                return Problem("Entity set 'ApplicationDBContext.DocumentInfo' is null.");
            }

            var documentInformation = new DocumentInformation
            {
                Documentname = requestDTO.DocumentName,
                Documenttype = requestDTO.DocumentType,
                Documentversion = requestDTO.DocumentVersion,
                Note = "",
                FileName = requestDTO.FileName,
                Status = "",
                IdFlight = requestDTO.IdFlight,  
            };

            // Kiểm tra xem IdFlight có tồn tại trong bảng AddFlight không
            var addFlight = _context.Addflights.Find(requestDTO.IdFlight);

            if (addFlight != null)
            {
                // Kiểm tra giá trị của IdFlight
                documentInformation.IdFlight = addFlight.FlightId;

                // Kiểm tra xem DocumentInformation có thể tự động thêm vào AddFlight không
                if (addFlight.DocumentInformation == null)
                {
                    addFlight.DocumentInformation = new List<DocumentInformation>();
                }

                // Kiểm tra xem GroupId có tồn tại trong bảng Group không
                var group = _context.Group.Find(requestDTO.GroupId);

                if (group != null)
                {
                    documentInformation.GroupId = group.GroupId;
                }
                else
                {
                    return BadRequest("Group with the specified GroupId not found.");
                }

                addFlight.DocumentInformation.Add(documentInformation);

                // Lưu lại thay đổi trong cả hai bảng
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDocumentInfo), new { id = documentInformation.Id }, documentInformation);
            }
            else
            {
                return BadRequest("AddFlight with the specified FlightId not found.");
            }
        }
        private string IncrementVersion(string currentVersion)
        {
            if (currentVersion == "1.0")
            {
                return currentVersion;
            }
            var parts = currentVersion.Split('.');
            if (parts.Length == 2 && int.TryParse(parts[1], out int minorVersion))
            {
                return $"{parts[0]}.{minorVersion + 1}";
            }

            return currentVersion;
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
