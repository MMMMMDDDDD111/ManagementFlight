﻿using FlightManagement.Models;
using FlightManagement.Models.Authentication.Login;
using FlightManagement.Models.Authentication.Signup;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FlightManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        
       
        public AuthenticationController(UserManager<IdentityUser> userManager,
              RoleManager<IdentityRole> roleManager, IConfiguration configuration)
            
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegisterUser registerUser, string role)
        {
            //check user exist
            var userExist = await _userManager.FindByEmailAsync(registerUser.Email);
            if (userExist != null)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new Response { Status = "ERROR", Message = "User already exist" });
            }

            //add user in databsae 
            IdentityUser user = new()
            {
                Email = registerUser.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = registerUser.Username
            };

            if(await _roleManager.RoleExistsAsync(role))
            {
                var result = await _userManager.CreateAsync(user, registerUser.Password);
                if (result.Succeeded)
                {
                    // Tài khoản tạo thành công
                    await _userManager.AddToRoleAsync(user, role);
                    return StatusCode(StatusCodes.Status200OK,
                        new Response { Status = "Success", Message = "User create successfully" });
                }
                else
                {
                    // Xử lý lỗi
                    string errors = string.Join(", ", result.Errors.Select(error => error.Description));
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new Response { Status = "Error", Message = $"User failed to create. Errors: {errors}" });
                }


            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                       new Response { Status = "Error", Message = "This roles doesnot exist." });
            }

            
            
            //assign a role 
        }
        
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromForm] LoginUser loginUser)
        {
            //checking the user.....
            var user = await _userManager.FindByNameAsync(loginUser.Username);
            if(user != null && await _userManager.CheckPasswordAsync(user, loginUser.Password))
            {
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                var userRoles = await _userManager.GetRolesAsync(user);
                foreach(var role in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, role));
                }
                
                var jwtToken = GetToken(authClaims);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expiration = jwtToken.ValidTo
                });
            }
            return Unauthorized();
         
        }
        
        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));
            return token;
        }


    }
}