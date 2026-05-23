namespace SchoolScheduleAvalonia.Models;

public sealed class ScheduleDto
{
    public int ScheduleId { get; set; }
    public DateOnly? Date { get; set; }
    public int? ClassId { get; set; }
    public int? DisciplineId { get; set; }
    public int? TimeSlotId { get; set; }
    public int? AssignmentId { get; set; }
}
