using HelpDeskApp.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly HelpDeskContext _context;

        public RolesController(HelpDeskContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetRoles()
        {
            var roles = await _context.Roles
                .Select(r => new
                {
                    r.RoleId,
                    r.RoleName
                })
                .ToListAsync();

            return Ok(roles);
        }
    }
}
