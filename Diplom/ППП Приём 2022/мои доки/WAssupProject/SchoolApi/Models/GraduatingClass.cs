using System.ComponentModel.DataAnnotations;

namespace SchoolApi.Models;

public class GraduatingClass
{
    public int GraduatingClassId { get; set; }

    [Required]
    [MaxLength(50)]
    public string ClassName { get; set; } = string.Empty;

    public int? ClassSize { get; set; }

    [Required]
    public int AcademicYearId { get; set; }
}
