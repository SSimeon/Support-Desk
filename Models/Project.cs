namespace HelpDeskApp.Models
{
    public class Project
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }

        public ICollection<UserProject> UserProjects { get; set; }
    }
}
