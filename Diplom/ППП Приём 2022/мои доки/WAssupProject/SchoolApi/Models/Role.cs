using System.ComponentModel.DataAnnotations;

namespace SchoolApi.Models;

public class Role
{
    public int RoleId { get; set; }

    [Required]
    [MaxLength(30)]
    public string RoleName { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? Description { get; set; }
}
