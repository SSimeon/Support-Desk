using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDeskApp.Models
{
    public class RoleModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleId { get; set; } 
        public string RoleName { get; set; } 

        public ICollection<UserRoles> UserRoles { get; set; }
    }
}
