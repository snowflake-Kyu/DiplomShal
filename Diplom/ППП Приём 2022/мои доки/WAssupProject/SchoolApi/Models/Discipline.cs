using System.ComponentModel.DataAnnotations;

namespace SchoolApi.Models;

public class Discipline
{
    public int DisciplineId { get; set; }

    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(255)]
    public string? ShortDescription { get; set; }
}
