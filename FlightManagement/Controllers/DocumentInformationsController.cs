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
using Microsoft.CodeAnalysis;
using System.Security.Cryptography;
using Nest;

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
                    CreateDate = DateTime.Now,
                    FlightNO = documentInfo.AddFlight?.Flightno ?? string.Empty
                };

                if (group != null)
                {
                    docsListItem.Creator = group.Creator;
                }

                docsList.Add(docsListItem);
            }

            return new JsonResult(new { Data = docsList });
        }

        [HttpGet("view-docs")]
        public async Task<ActionResult<List<ViewDoc>>> GetDocumentViews()
        {
            var documentInfos = await _context.DocumentInfo
                .Include(af => af.AddFlight)
                .Include(doc => doc.Groups)
                .ThenInclude(group => group.Members)
                .Include(doc => doc.UpdateVersions)
                .ToListAsync();

            if (documentInfos == null || !documentInfos.Any())
            {
                return NotFound("Documents not found.");
            }

            var viewDocs = documentInfos.Select(documentInfo => new ViewDoc
            {
                Id = documentInfo.Id,
                Title = documentInfo.FileName,
                Type = documentInfo.Documenttype,
                CreateDate = documentInfo.AddFlight?.Date ?? DateTime.Now,
                Permissions = documentInfo.Groups.Permissions,
                Creator = documentInfo.Groups.Creator != null ? $"By: {documentInfo.Groups.Creator}" : "Creator not available",
                UpdatedVersions = documentInfo.Documentversion?.Select(version => new UpdateVersion
                {
                    DocID = documentInfo.Id,
                    Version = documentInfo.Documentversion,
                    Date = documentInfo.AddFlight?.Date?.ToString("yyyy-MM-dd") ?? "N/A"
                }).ToList()
            }).ToList();

            return Ok(viewDocs);
        }


        [HttpPost]
        public async Task<ActionResult<DocumentInformation>> CreateDocument([FromForm] DocumentInfoDTO createDTO, IFormFile file)
        {
            if (createDTO == null || ModelState.IsValid == false)
            {
                return BadRequest("Invalid input data");
            }

            if (IsCreateAllowed())
            {
                try
                {
                    var documentInformation = new DocumentInformation
                    {
                        Documentname = createDTO.DocumentName,
                        Documenttype = createDTO.DocumentType,
                        Note = "",
                        FileName = createDTO.FileName,
                        Status = "",
                        IdFlight = createDTO.IdFlight,
                    };

                    var addFlight = _context.Addflights.Find(createDTO.IdFlight);

                    if (addFlight != null)
                    {
                        documentInformation.IdFlight = addFlight.FlightId;

                        if (addFlight.DocumentInformation == null)
                        {
                            addFlight.DocumentInformation = new List<DocumentInformation>();
                        }

                        var group = _context.Group.Find(createDTO.GroupId);

                        if (group != null)
                        {
                            documentInformation.GroupId = group.GroupId;
                        }
                        else
                        {
                            return BadRequest("Group with the specified GroupId not found.");
                        }

                        addFlight.DocumentInformation.Add(documentInformation);

                        await _context.SaveChangesAsync();

                        return CreatedAtAction(nameof(GetDocumentInfo), new { id = documentInformation.Id }, documentInformation);
                    }
                    else
                    {
                        return BadRequest("AddFlight with the specified FlightId not found.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating document: {ex.Message}");
                    return StatusCode(500, "Internal server error");
                }
            }
            else
            {
                return BadRequest("Creating a new document is not allowed at this time.");
            }
        }

        private bool IsCreateAllowed()
        {
            DateTime thresholdTime = DateTime.UtcNow.AddHours(-2);
            return DateTime.UtcNow < thresholdTime;
        }

        [HttpPut]
        public async Task<ActionResult<DocumentInformation>> UpdateDocumentInformation([FromForm] DocumentInfoDTO updateDTO, IFormFile file)
        {
            if (updateDTO == null || ModelState.IsValid == false)
            {
                return BadRequest("Invalid input data");
            }

            var existingDocument = await _context.DocumentInfo
                .Include(doc => doc.AddFlight)
                .FirstOrDefaultAsync(doc => doc.Id == updateDTO.ID);

            if (existingDocument == null)
            {
                return NotFound("Document not found");
            }

            if (IsUpdateAllowed(existingDocument.UpdateDate))
            {
                try
                {
                    existingDocument.Documentname = updateDTO.DocumentName;
                    existingDocument.Documenttype = updateDTO.DocumentType;
                    existingDocument.IdFlight = updateDTO.IdFlight;
                    existingDocument.AddFlight = await _context.Addflights.FirstOrDefaultAsync(af => af.FlightId == updateDTO.IdFlight);
                    existingDocument.GroupId = updateDTO.GroupId;
                    existingDocument.Groups = await _context.Group.FirstOrDefaultAsync(g => g.GroupId == updateDTO.GroupId);

                    existingDocument.Documentversion = IncrementVersion(existingDocument.Documentversion);

                    await _context.SaveChangesAsync();

                    return Ok(new { Id = existingDocument.Id, Message = "Document updated successfully" });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating document: {ex.Message}");
                    return StatusCode(500, "Internal server error");
                }
            }
            else
            {
                return BadRequest("Updating the document is not allowed after a certain time.");
            }
        }

        private bool IsUpdateAllowed(DateTime? updateDate)
        {
            DateTime thresholdTime = DateTime.UtcNow.AddHours(-2);
            return updateDate == null || updateDate < thresholdTime;
        }

        private string IncrementVersion(string currentVersion)
        {
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
