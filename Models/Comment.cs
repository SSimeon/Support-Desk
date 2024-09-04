namespace HelpDeskApp.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public int TicketId { get; set; }
        public string CommentText { get; set; }
        public DateTime CommentDateTime { get; set; } = DateTime.Now;
        public int? UserId { get; set; }  
    }
}
