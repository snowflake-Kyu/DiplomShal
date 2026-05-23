using System.ComponentModel.DataAnnotations;

namespace SchoolApi.Models;

public class AppUser
{
    public int UserId { get; set; }

    [Required]
    [MaxLength(30)]
    public string Login { get; set; } = string.Empty;

    [Required]
    [MaxLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public int RoleId { get; set; }
}
