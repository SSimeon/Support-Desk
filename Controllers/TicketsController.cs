using HelpDeskApp.Data;
using HelpDeskApp.DTO;
using HelpDeskApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

namespace HelpDeskApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly HelpDeskContext _context;

        public TicketsController(HelpDeskContext context)
        {
            _context = context;
        }

        [HttpGet("{username}")]
        public async Task<ActionResult> GetUserDetails(string username)
        {
            var user = await _context.Users
                .Where(u => u.Username == username)
                .Select(u => new
                {
                    u.UserId,
                    u.FullName,
                    u.Email
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectsByUsername([FromQuery] string username)
        {
            var user = await _context.Users
                .Where(u => u.Username == username)
                .Select(u => u.UserId)
                .FirstOrDefaultAsync();

            if (user == 0)
            {
                return NotFound(new { message = "User not found" });
            }

            var projectIds = await _context.UserProjects
                .Where(up => up.UserID == user)
                .Select(up => up.ProjectID)
                .ToListAsync();

            if (projectIds.Count == 0)
            {
                return NotFound(new { message = "No projects found for this user" });
            }

            var projects = await _context.Projects
                .Where(p => projectIds.Contains(p.ProjectID))
                .Select(p => new { p.ProjectID, p.ProjectName })
                .ToListAsync();

            return Ok(projects);
        }


        [HttpGet]
        [Route("projects")]
        public async Task<IActionResult> GetAllProjects()
        {
            var projects = await _context.Projects
                .Select(p => new
                {
                    p.ProjectID,
                    p.ProjectName
                })
                .ToListAsync();

            return Ok(projects);
        }


        [HttpPost]
        [Route("upload-ticket-picture")]
        public async Task<IActionResult> UploadTicketPicture([FromForm] IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest(new { Message = "No file uploaded." });
            }

            try
            {
                var fileName = ContentDispositionHeaderValue.Parse(imageFile.ContentDisposition).FileName.TrimStart('\"').TrimEnd('\"');
                string newPath = @"C:\Users\User\Desktop\TicketPictures";

                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }

                string[] allowedImageExtensions = { ".jpg", ".jpeg", ".png" };
                if (!allowedImageExtensions.Contains(Path.GetExtension(fileName)))
                {
                    return BadRequest(new { Message = "Only .jpg, .jpeg, and .png files are allowed." });
                }

                string newFileName = Guid.NewGuid() + Path.GetExtension(fileName);
                string fullFilePath = Path.Combine(newPath, newFileName);

                using (var stream = new FileStream(fullFilePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                return Ok(new
                {
                    PictureUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/StaticFiles/{newFileName}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while uploading the file.", Error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/Tickets")]
        public async Task<IActionResult> CreateTicket([FromForm] CreateTicketDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string pictureUrl = null;
            if (dto.File != null)
            {
                var uploadResponse = await UploadTicketPicture(dto.File);
                if (uploadResponse is BadRequestObjectResult badRequest)
                {
                    return badRequest;
                }

                var result = (OkObjectResult)uploadResponse;
                pictureUrl = ((dynamic)result.Value).PictureUrl;
            }

            var ticket = new TicketsModel
            {
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                CreatedById = dto.UserId,
                ProjectId = dto.ProjectId,
                PictureUrl = pictureUrl,
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return Ok(ticket);
        }


        [HttpGet]
        [Route("api/TicketsByUsername")]
        public async Task<IActionResult> GetTicketsByUsername([FromQuery] string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest(new { Message = "Username is required." });
            }

            var user = await _context.Users
                .Where(u => u.Username == username)
                .Select(u => new { u.UserId })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            var tickets = await _context.Tickets
                .Where(t => t.CreatedById == user.UserId)
                .Select(t => new
                {
                    t.TicketId,
                    t.Title,
                    t.Description,
                    t.Priority,
                    t.CreatedDate,
                    t.Status,
                    ProjectName = t.Project.ProjectName,
                    AssignedToName = t.AssignedTo != null ? t.AssignedTo.FullName : null,
                    t.PictureUrl,
                    user.UserId,
                    ClosingNote = string.IsNullOrEmpty(t.ClosingNote) ? null : t.ClosingNote
                })
                .ToListAsync();

            return Ok(tickets);
        }

        [HttpGet]
        [Route("pending-tickets")]
        public async Task<IActionResult> GetPendingTickets()
        {
            var priorityOrder = new Dictionary<string, int>
            {
                { "High", 1 },
                { "Medium", 2 },
                { "Low", 3 }
            };

            var pendingTicketsQuery = await _context.Tickets
                .Where(t => t.Status == "Pending")
                .Include(t => t.Project) 
                .Include(t => t.CreatedBy) 
                .ToListAsync();

            var orderedTickets = pendingTicketsQuery
                .AsEnumerable() 
                .OrderBy(t => priorityOrder.ContainsKey(t.Priority) ? priorityOrder[t.Priority] : 4) 
                .Select(t => new
                {
                    t.TicketId,
                    t.Title,
                    CreatedDate = t.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"), 
                    ProjectName = t.Project != null ? t.Project.ProjectName : "N/A", 
                    CreatedBy = t.CreatedBy != null ? t.CreatedBy.Username : "Unknown", 
                    t.Priority
                })
                .ToList(); 

            return Ok(orderedTickets);
        }

        [HttpGet]
        [Route("handlers")]
        public async Task<IActionResult> GetHandlers()
        {
            var handlers = await _context.UserRoles
                .Where(ur => ur.Role.RoleName == "Handler" && ur.User.IsApproved == true)
                .Select(ur => new
                {
                    ur.User.UserId,
                    ur.User.Username,
                    ur.User.FullName
                })
                .ToListAsync();

            return Ok(handlers);
        }



        [HttpPost]
        [Route("assign-ticket")]
        public async Task<IActionResult> AssignTicket([FromBody] AssignTicketRequest request)
        {
            if (request == null || request.TicketId == 0 || request.UserId == 0 || string.IsNullOrEmpty(request.AssignerUsername) || request.ExpectedFinishDate == DateTime.MinValue)
            {
                return BadRequest("Invalid request data.");
            }

            var ticket = await _context.Tickets.FindAsync(request.TicketId);
            if (ticket == null)
            {
                return NotFound("Ticket not found.");
            }

            var assigner = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.AssignerUsername);
            if (assigner == null)
            {
                return NotFound("Assigner not found.");
            }

            ticket.AssignedToId = request.UserId;
            ticket.AssignedById = assigner.UserId;
            ticket.ExpectedFinishDate = request.ExpectedFinishDate;
            ticket.Status = "Assigned";

            await _context.SaveChangesAsync();

            return Ok("Ticket assigned successfully.");
        }

        [HttpGet]
        [Route("assigned-to-user/{username}")]
        public async Task<IActionResult> GetAssignedTickets(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest("Invalid username.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var userId = user.UserId;

            var tickets = await _context.Tickets
                .Where(t => t.AssignedToId == userId)
                .Select(t => new
                {
                    t.TicketId,
                    t.Title,
                    t.Status,
                    t.CreatedDate,
                    CreatedBy = _context.Users.Where(u => u.UserId == t.CreatedById).Select(u => u.Username).FirstOrDefault(),
                    AssignedBy = _context.Users.Where(u => u.UserId == t.AssignedById).Select(u => u.Username).FirstOrDefault(),
                    t.Priority,
                    t.ExpectedFinishDate,
                    t.PictureUrl,
                    t.Description,
                    ClosingNote = string.IsNullOrEmpty(t.ClosingNote) ? null : t.ClosingNote
                })
                .ToListAsync();

            var priorityOrder = new Dictionary<string, int>
                {
                    { "High", 1 },
                    { "Medium", 2 },
                    { "Low", 3 }
                };

            var orderedTickets = tickets.OrderBy(t => priorityOrder[t.Priority]).ToList();

            

            return Ok(orderedTickets);
        }


        [HttpPut]
        [Route("start-ticket/{ticketId}")]
        public async Task<IActionResult> StartTicket(int ticketId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);

            if (ticket == null)
            {
                return NotFound("Ticket not found.");
            }

            if (ticket.Status != "Assigned")
            {
                return BadRequest("Ticket is not in a state that can be started.");
            }

            ticket.Status = "In Progress";
            await _context.SaveChangesAsync();

            return Ok("Ticket started successfully.");
        }

        [HttpPut]
        [Route("close-ticket/{ticketId}")]
        public async Task<IActionResult> CloseTicket(int ticketId, [FromBody] TicketCloseRequestModel request)
        {
            if (ticketId == 0)
            {
                return BadRequest("Invalid Ticket ID.");
            }

            var ticket = await _context.Tickets.FindAsync(ticketId);

            if (ticket == null)
            {
                return NotFound("Ticket not found.");
            }

            ticket.ClosingNote = request.HandlerNote;
            ticket.FinishDate = DateTime.Now;
            ticket.Status = "Closed";

            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();

            return Ok("Ticket has been successfully closed.");
        }

    }
}
