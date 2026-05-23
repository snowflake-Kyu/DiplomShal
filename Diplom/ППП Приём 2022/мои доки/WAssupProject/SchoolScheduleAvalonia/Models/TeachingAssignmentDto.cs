namespace SchoolScheduleAvalonia.Models;

public sealed class TeachingAssignmentDto
{
    public int AssignmentId { get; set; }
    public int TeacherId { get; set; }
    public int DisciplineId { get; set; }
    public int? AuditoriumId { get; set; }
    public string? Comment { get; set; }
}
