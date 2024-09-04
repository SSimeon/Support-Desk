using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelpDeskApp.Data;
using HelpDeskApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelpDeskApp.DTO;

namespace HelpDeskApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignInController : ControllerBase
    {
        private readonly HelpDeskContext _context;

        public SignInController(HelpDeskContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDTO registrationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == registrationDto.Username);

            if (existingUser != null)
            {
                return Conflict("Username already exists.");
            }

            
            var user = new UserModel
            {
                FullName = registrationDto.FullName,
                Email = registrationDto.Email,
                Username = registrationDto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registrationDto.Password),
                IsApproved = false 
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var roles = registrationDto.Roles.Select(roleId => new UserRoles
            {
                UserId = user.UserId,
                RoleId = roleId
            }).ToList();

            _context.UserRoles.AddRange(roles);
            await _context.SaveChangesAsync();

            return Ok("Registration successful. Please wait for approval.");
        }

        [HttpGet]
        [Route("unapproved-registrations")]
        public async Task<IActionResult> GetUnapprovedRegistrations()
        {
            var unapprovedUsers = await _context.Users
                .Where(u => !u.IsApproved && !u.IsDennied)
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    u.Email,
                    u.FullName,
                    Roles = u.UserRoles.Select(ur => ur.Role.RoleName).ToList()
                })
                .ToListAsync();

            return Ok(unapprovedUsers);
        }

        [HttpPost]
        [Route("approve-registration")]
        public async Task<IActionResult> ApproveRegistration([FromBody] ApproveRegistrationRequest request)
        {
            if (request.UserId == 0)
            {
                return BadRequest("Invalid User ID.");
            }

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.IsApproved = true;

            if (request.ProjectIds != null && request.ProjectIds.Any())
            {
                foreach (var projectId in request.ProjectIds)
                {
                    var userProject = new UserProject
                    {
                        UserID = request.UserId,
                        ProjectID = projectId
                    };

                    _context.UserProjects.Add(userProject);
                }
            }

            await _context.SaveChangesAsync();

            return Ok("Registration approved and projects assigned.");
        }

        [HttpDelete]
        [Route("deny-registration/{userId}")]
        public async Task<IActionResult> DenyRegistration(int userId)
        {
            if (userId == 0)
            {
                return BadRequest("Invalid User ID.");
            }

            var userRoles = _context.UserRoles.Where(ur => ur.UserId == userId);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.IsDennied = true;


            if (!userRoles.Any())
            {
                return NotFound("No roles found for the user.");
            }

            _context.UserRoles.RemoveRange(userRoles);

            await _context.SaveChangesAsync();

            return Ok("User roles removed and registration denied.");
        }



    }
}
