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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FlightManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly LoginUser loginUser;

        public class GroupDto
        {
            public string GroupName { get; set; }
            public int? Member { get; set; }
            public int? Permissions { get; set; }
            public string? Creator { get; set; }
            public List<string> SelectedUsernames { get; set; }
        }


        public GroupsController(ApplicationDBContext context) => _context = context;
        // GET: api/<GroupsController>
        [HttpGet]
        public async Task<IActionResult> GetAll(int? page)
        {
            if (_context.Group == null)
            {
                return NotFound();
            }
  
            int pageSize = 5; 
            int pageNumber = (page ?? 1);
            var groups = await _context.Group.Include(g => g.Members).ToListAsync();  
            var pagedGroups = groups.ToPagedList(pageNumber, pageSize);

            var groupDtos = pagedGroups.Select(group => new GroupDto
            {
                GroupName = group.GroupName,
                Member = group.Member,
                Permissions = (int?)group.Permissions,
                Creator = group.Creator,

            }).ToList();

            return new JsonResult(groupDtos);
        }

        // GET api/<GroupsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<GroupsController>
        private Groups ConvertToGroups(Groups dto)
        {
    
            var group = new Groups
            {
                GroupName = dto.GroupName,
                Permissions = dto.Permissions,  
                Member = dto.Member,
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
                if (model.SelectedUsernames == null || !model.SelectedUsernames.Any())
                {
                    return BadRequest("Please select at least one member.");
                }

                var group = new Groups
                {
                    GroupName = model.GroupName,
                    Permissions = (NewFolder.EnumExtensions.Permission?)model.Permissions,
                    Member = model.Member,
                    Creator = model.Creator,
                    Members = new List<LoginUser>()
                };

                foreach (var username in model.SelectedUsernames)
                {
                    var user = _context.loginUsers.FirstOrDefault(u => u.Username == username);
                    if (user != null)
                    {
                        group.Members.Add(user);
                    }
                }

                _context.Group.Add(group);
                _context.SaveChanges();

                return CreatedAtAction(nameof(GetAll), new { id = group.GroupId }, group);
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


        // PUT api/<GroupsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<GroupsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

