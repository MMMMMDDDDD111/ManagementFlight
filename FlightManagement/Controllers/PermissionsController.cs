using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightManagement.Models;
using FlightManagement.Models.Management_Flight;
using FlightManagement.Models.Authentication.Login;

namespace FlightManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public PermissionsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/Permissions
        // DTO (Data Transfer Object) cho Creator
        public class CreatorDTO
        {
            public string? Username { get; set; }
            public string? GroupName { get; set; }
            public int? Members { get; set; }
            public DateTime? CreateDate { get; set; }
            public string? Note { get; set; }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CreatorDTO>>> GetCreators()
        {
            if (_context.Permiss == null)
            {
                return NotFound();
            }

            // Lấy danh sách Permission từ cơ sở dữ liệu với trường Creator
            var permissions = await _context.Permiss.Include(p => p.Creator).ToListAsync();

            // Chuyển đổi từ Permission sang CreatorDTO
            var creators = permissions.Select(p => new CreatorDTO
            {
                Username = p.Creator?.Username,
                GroupName =p.GroupName,
                Members = p.Members,
                CreateDate = DateTime.Now.Date,
                Note = p.Note
            }).ToList();

            return creators;
        }


        // GET: api/Permissions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Permission>> GetPermission(int id)
        {
          if (_context.Permiss == null)
          {
              return NotFound();
          }
            var permission = await _context.Permiss.FindAsync(id);

            if (permission == null)
            {
                return NotFound();
            }

            return permission;
        }

        // PUT: api/Permissions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPermission(int id, Permission permission)
        {
            if (id != permission.Id)
            {
                return BadRequest();
            }

            _context.Entry(permission).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PermissionExists(id))
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

        // POST: api/Permissions
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Permission>> PostPermission(Permission permission)
        {
            if (_context.Permiss == null)
            {
                return Problem("Entity set 'ApplicationDBContext.Permiss' is null.");
            }

            // Tìm DocumentInfo dựa trên điều kiện cụ thể, ví dụ tìm theo Documentname
            var documentInfo = await _context.DocumentInfo.FirstOrDefaultAsync(d => d.Documentname == "Tên tài liệu");

            if (documentInfo != null)
            {
                // Gán giá trị Note từ DocumentInfo vào Permission
                permission.Note = documentInfo?.Note;
            }

            // Tạo đối tượng Creator từ tên người dùng của `LoginUser`
            var creatorUsername = permission.CreatorUsername; // Thay thế "Tên người dùng" bằng tên người dùng cụ thể
            permission.Creator = new LoginUser { Username = creatorUsername, Password = "defaultPassword" };


            _context.Permiss.Add(permission);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPermission", new { id = permission.Id }, permission);
        }



        // DELETE: api/Permissions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            if (_context.Permiss == null)
            {
                return NotFound();
            }
            var permission = await _context.Permiss.FindAsync(id);
            if (permission == null)
            {
                return NotFound();
            }

            _context.Permiss.Remove(permission);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PermissionExists(int id)
        {
            return (_context.Permiss?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
