namespace HelpDeskApp.DTO
{
    public class CreateTicketDTO
    {
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public IFormFile? File { get; set; } // For file upload
    }
}
