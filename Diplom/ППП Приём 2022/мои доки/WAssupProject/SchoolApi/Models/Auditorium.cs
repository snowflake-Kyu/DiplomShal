using System.ComponentModel.DataAnnotations;

namespace SchoolApi.Models;

public class Auditorium
{
    public int AuditoriumId { get; set; }

    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(255)]
    public string? Description { get; set; }
}
