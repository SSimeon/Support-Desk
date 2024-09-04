using System.Data;

namespace HelpDeskApp.Models
{
    public class UserRoles
    {
        public int UserId { get; set; }
        public UserModel User { get; set; }

        public int RoleId { get; set; }
        public RoleModel Role { get; set; }
    }
}
