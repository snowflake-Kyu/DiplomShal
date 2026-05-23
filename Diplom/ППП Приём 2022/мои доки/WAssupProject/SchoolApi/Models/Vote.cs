using System.ComponentModel.DataAnnotations;

namespace SchoolApi.Models;

public class Vote
{
    public int VoteId { get; set; }

    [Required]
    public int DisciplineId { get; set; }

    [Required]
    public int RateId { get; set; }
}
