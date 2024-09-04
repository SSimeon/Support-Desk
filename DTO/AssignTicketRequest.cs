public class AssignTicketRequest
{
    public int TicketId { get; set; }
    public int UserId { get; set; }
    public string AssignerUsername { get; set; }
    public DateTime ExpectedFinishDate { get; set; }
}
