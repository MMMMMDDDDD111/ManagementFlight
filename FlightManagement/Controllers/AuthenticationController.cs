using FlightManagement.Models;
using FlightManagement.Models.Authentication.Login;
using FlightManagement.Models.Authentication.Signup;
using Microsoft.AspNetCore.Authorization;
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
        [Route("Register")]
        public async Task<IActionResult> Register([FromForm] RegisterUser registerUser, string role)
        {
            //check user exist
            var userExist = await _userManager.FindByNameAsync(registerUser.Username);
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
                        new Response { Status = "Error", Message = result.ToString() });
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                       new Response { Status = "Error", Message = "This roles doesnot exist." });
            }

        }
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromForm] LoginUser loginUser)
        {
            // Kiểm tra người dùng
            var user = await _userManager.FindByNameAsync(loginUser.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, loginUser.Password))
            {
                var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

                var userRoles = await _userManager.GetRolesAsync(user);
                foreach (var role in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, role));
                }

                var jwtToken = CreateToken(authClaims);

                return Ok(jwtToken); // Trả về chuỗi JWT
            }
            return Unauthorized();
        }

        private string CreateToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));

            var token = new JwtSecurityToken(
                expires: DateTime.Now.AddHours(2),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha512Signature)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        [HttpPost]
        [Route("Change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword changePassword)
        {
            var user = await _userManager.FindByNameAsync(changePassword.Username);
            if (user == null)
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "User does not exist!!!"});
            if (string.Compare(changePassword.NewPassword, changePassword.ConfirmNewPassword) != 0)
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "The new password and comfirm new password does not match!!!" });
            var result = await _userManager.ChangePasswordAsync(user,changePassword.CurrentPassword,changePassword.NewPassword);
            if (!result.Succeeded)
            {
                var errors = new List<string>();
                
                foreach(var error in result.Errors)
                {
                    errors.Add(error.Description);
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = string.Join(",", errors)});
            }
            return Ok(new Response { Status="Success", Message="Password successfully changed."});


        }


    }
}
