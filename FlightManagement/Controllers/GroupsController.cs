using Elasticsearch.Net;
using FlightManagement.Models;
using FlightManagement.Models.Authentication.Login;
using FlightManagement.Models.Management_Flight;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using X.PagedList;
using static FlightManagement.Controllers.AddFlightsController;
using static FlightManagement.NewFolder.EnumExtensions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FlightManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly LoginUser loginUser;
        private readonly DocumentInformation documentInformation;

        public GroupsController(ApplicationDBContext context) => _context = context;
        // GET: api/<GroupsController>
        [HttpGet("GetGroupDetails")]
        [ProducesResponseType(typeof(IEnumerable<GroupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGroupDetails([FromQuery] int? page)
        {
            if (_context.Group == null)
            {
                return NotFound();
            }

            int pageSize = 5;
            int pageNumber = page ?? 1;

            var pagedGroups = await _context.Group
                .Include(g => g.Members) 
                .AsNoTracking()
                .OrderBy(g => g.GroupId) 
                .ToPagedListAsync(pageNumber, pageSize);

            var groupDtos = pagedGroups.Select(group => new GroupDTO
            {
                GroupId = group.GroupId,
                GroupName = group.GroupName,
                Member = group.Members?.Count ?? 0,
                Permissions = group.Permissions,
                Creator = group.Creator,
                Username = group.Members?.Select(member => member.Username).ToList() ?? new List<string>()
            }).ToList();

            return new JsonResult(groupDtos);
        }


        // POST api/<GroupsController>
        private Groups ConvertToGroups(Groups dto)
        {
    
            var group = new Groups
            {
                
                GroupName = dto.GroupName,
                Permissions = dto.Permissions,  
                GroupId = dto.GroupId,
                Creator = dto.Creator
            };

            return group;
        }

        [HttpPost]
        public async Task<ActionResult> CreateGroup([FromForm] GroupDto model)
        {
            if (ModelState.IsValid)
            {
                if (model.Username == null || !model.Username.Any())
                {
                    return BadRequest("Please select at least one member.");
                }

                var group = new Groups
                {
                    GroupId = model.GroupId,
                    GroupName = model.GroupName,
                    Permissions = model.Permissions,
                    Creator = model.Creator,
                    Members = new List<LoginUser>()
                };

                foreach (var username in model.Username)
                {
                    var user = _context.loginUsers.FirstOrDefault(u => u.Username == username);
                    if (user != null)
                    {
                        group.Members.Add(user);
                    }
                }

                _context.Group.Add(group);
                await _context.SaveChangesAsync();


                return CreatedAtAction(nameof(GetGroupDetails), new { id = group.GroupId }, group);
            }

            return BadRequest("Invalid model data.");
        }


        [HttpGet("SearchGroupsByName")]
        public async Task<ActionResult<IEnumerable<Groups>>> SearchGroupsByName(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                return BadRequest("Group name cannot be empty.");
            }

            var groups = await _context.Group
                .Where(g => g.GroupName == groupName)
                .ToListAsync();

            if (groups == null || !groups.Any())
            {
                return NotFound("No groups found with the provided name.");
            }

            return groups;
        }

    }
}

