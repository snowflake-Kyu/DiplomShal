using System.ComponentModel.DataAnnotations;

namespace SchoolApi.Models;

public class TeachingAssigmentHistory
{
    public int TeachingAssigmentHistoryId { get; set; }

    [Required]
    [MaxLength(50)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Email { get; set; }

    [Required]
    [MaxLength(50)]
    public string DisciplineName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string AcademicYear { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Comment { get; set; }
}
