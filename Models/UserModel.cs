using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Net.Sockets;

namespace HelpDeskApp.Models
{
    public class UserModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public bool IsApproved { get; set; } = false;

        public bool IsDennied { get; set; } = false;

        public ICollection<UserRoles> UserRoles { get; set; }

        public ICollection<TicketsModel> CreatedTickets { get; set; } 
        public ICollection<TicketsModel> AssignedTickets { get; set; } 
        public ICollection<TicketsModel> AssignedByTickets { get; set; } 
        public ICollection<TicketHistoryModel> TicketHistories { get; set; }

        public ICollection<UserProject> UserProjects { get; set; }

    }
}
