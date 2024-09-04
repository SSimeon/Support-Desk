namespace HelpDeskApp.DTO
{
    public class ApproveRegistrationRequest
    {
        public int UserId { get; set; }
        public List<int> ProjectIds { get; set; } // List of project IDs (can be null or empty)
    }
}
