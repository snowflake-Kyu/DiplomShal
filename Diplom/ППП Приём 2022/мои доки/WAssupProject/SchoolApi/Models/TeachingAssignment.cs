using System.ComponentModel.DataAnnotations;

namespace SchoolApi.Models;

public class TeachingAssignment
{
    public int AssignmentId { get; set; }

    [Required]
    public int TeacherId { get; set; }

    [Required]
    public int DisciplineId { get; set; }

    public int? AuditoriumId { get; set; }

    [MaxLength(200)]
    public string? Comment { get; set; }
}
