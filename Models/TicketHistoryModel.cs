using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace HelpDeskApp.Models
{
    public class TicketHistoryModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HistoryId { get; set; }
        public int TicketId { get; set; }
        public TicketsModel Ticket { get; set; }

        public string Status { get; set; } // Status at the time of the change
        public DateTime ChangeDate { get; set; } = DateTime.UtcNow;

        public int ChangedById { get; set; }
        public UserModel ChangedBy { get; set; } // User who made the change

        public string Comment { get; set; } // Optional description of the change
    }
}
