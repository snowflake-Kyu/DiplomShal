using System.ComponentModel.DataAnnotations;

namespace SchoolApi.Models;

public class AcademicYear
{
    public int AcademicYearId { get; set; }

    [Required]
    public int Year { get; set; }

    [MaxLength(100)]
    public string? Comment { get; set; }
}
