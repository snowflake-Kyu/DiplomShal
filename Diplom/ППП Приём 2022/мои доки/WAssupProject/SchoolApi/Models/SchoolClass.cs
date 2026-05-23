using System.ComponentModel.DataAnnotations;

namespace SchoolApi.Models;

public class SchoolClass
{
    public int ClassId { get; set; }

    [MaxLength(50)]
    public string? ClassName { get; set; }

    public int? ClassSize { get; set; }
}
