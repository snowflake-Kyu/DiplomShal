using System.ComponentModel.DataAnnotations;

namespace SchoolApi.Models;

public class Rate
{
    public int RateId { get; set; }

    [Required]
    [MaxLength(50)]
    public string RateName { get; set; } = string.Empty;
}
