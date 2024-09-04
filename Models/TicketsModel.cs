using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDeskApp.Models
{
    public class TicketsModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TicketId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public string? ClosingNote { get; set; }

        public DateTime? ExpectedFinishDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public string Status { get; set; } = "Pending"; // E.g., "Open", "Assigned", "In Progress", "Closed"
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int CreatedById { get; set; }
        public UserModel CreatedBy { get; set; } // Customer who created the ticket

        public int? AssignedToId { get; set; }
        public UserModel AssignedTo { get; set; } // Employee assigned to the ticket

        public int? AssignedById { get; set; }
        public UserModel AssignedBy { get; set; } // TicketManager who assigned the ticket

        public string Priority { get; set; } 

        public int ProjectId { get; set; } // Foreign Key for Project
        public Project Project { get; set; } // Navigation property to Project

        public string? PictureUrl { get; set; } // URL for the uploaded picture

        public ICollection<TicketHistoryModel> TicketHistories { get; set; } // Track
    }
}
