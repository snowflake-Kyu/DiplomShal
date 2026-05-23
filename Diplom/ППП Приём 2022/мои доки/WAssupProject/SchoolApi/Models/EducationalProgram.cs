using System.ComponentModel.DataAnnotations;

namespace SchoolApi.Models;

public class EducationalProgram
{
    public int ProgramId { get; set; }
    public int? DisciplineId { get; set; }
    public int? ClassId { get; set; }

    [MaxLength(255)]
    public string? ShortDescription { get; set; }

    public int? HoursCount { get; set; }
    public int Quarter1Hours { get; set; }
    public int Quarter2Hours { get; set; }
    public int Quarter3Hours { get; set; }
    public int Quarter4Hours { get; set; }
}
