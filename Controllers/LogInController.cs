using HelpDeskApp.Data;
using HelpDeskApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity.Data;
using HelpDeskApp.DTO;


namespace HelpDeskApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogInController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly HelpDeskContext _context;

        public LogInController(IConfiguration config, HelpDeskContext context)
        {
            _config = config;
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> LogIn([FromBody] LogInRequestDTO loginRequest)
        {
            var user = await Authenticate(loginRequest);
            if (user == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            if (!user.IsApproved)
            {
                return Unauthorized("User registration not approved yet!");
            }

            var roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList();
            var token = GenerateJwtToken(user, roles);

            var userRole = roles.FirstOrDefault();
            return Ok(new
            {
                Token = token,
                Roles = roles,
                RedirectUrl = GetRedirectUrl(userRole)
            });
        }

        private async Task<UserModel> Authenticate(LogInRequestDTO loginRequest)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == loginRequest.Username);
            if (user != null && BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            {
                return user;
            }

            return null;
        }

        private string GenerateJwtToken(UserModel user, IList<string> roles)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FullName)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private string GetRedirectUrl(string role)
        {
      
            switch (role)
            {
                case "Client":
                    return "/";
                case "Assigner":
                    return "/assigner";
                case "Handler":
                    return "/handler";
                default:
                    return "/";
            }
        }
    }
}
