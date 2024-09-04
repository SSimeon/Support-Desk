using HelpDeskApp.Data;
using HelpDeskApp.DTO;
using HelpDeskApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly HelpDeskContext _context;

        public CommentController(HelpDeskContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("/create-comment")]
        public async Task<IActionResult> AddComment([FromBody] CommentCreationDTO commentDto)
        {
            if (commentDto == null)
            {
                return BadRequest("Invalid comment data.");
            }

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == commentDto.username);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var comment = new Comment
            {
                TicketId = commentDto.ticketId,
                CommentText = commentDto.commentText,
                CommentDateTime = DateTime.Now,
                UserId = user.UserId,
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(comment);
        }

        [HttpGet]
        [Route("{ticketId}")]

        public async Task<IActionResult> GetAllCommentsById(int ticketId)
        {
            if(ticketId == 0)
            {
                return BadRequest("Invalid ticket ID");
            }

            var comments = await _context.Comments
                .Where(c => c.TicketId == ticketId)
                .Join(_context.Users,
                    comment => comment.UserId,
                    user => user.UserId,
                        (comment, user) => new
                            {
                                comment.CommentId,
                                comment.TicketId,
                                comment.CommentText,
                                comment.CommentDateTime,
                                Username = user.Username
                })
                .ToListAsync();

            return Ok(comments);


        }


    }
}
