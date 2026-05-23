using System.ComponentModel.DataAnnotations;

namespace SchoolApi.Models;

public class StudentGroup
{
    public int GroupId { get; set; }

    [MaxLength(100)]
    public string? GroupName { get; set; }

    public int? GroupSize { get; set; }
}
