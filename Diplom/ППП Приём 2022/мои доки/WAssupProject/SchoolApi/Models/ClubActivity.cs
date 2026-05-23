namespace SchoolApi.Models;

public class ClubActivity
{
    public int ClubActivityId { get; set; }
    public int? GroupId { get; set; }
    public int? TeacherId { get; set; }
    public DateOnly? Date { get; set; }
}
