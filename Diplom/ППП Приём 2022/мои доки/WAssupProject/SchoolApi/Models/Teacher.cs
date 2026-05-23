using System.ComponentModel.DataAnnotations;

namespace SchoolApi.Models;

public class Teacher
{
    public int TeacherId { get; set; }

    [MaxLength(100)]
    public string? FullName { get; set; }

    [MaxLength(100)]
    [EmailAddress]
    public string? Email { get; set; }
}
