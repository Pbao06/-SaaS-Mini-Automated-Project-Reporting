using System.ComponentModel.DataAnnotations;

namespace SaaS.Api.Models.DTOs.Admin
{
    public class AssignRoleRequest
    {
        [Required]
        public string RoleId { get; set; } = string.Empty;
    }
}
