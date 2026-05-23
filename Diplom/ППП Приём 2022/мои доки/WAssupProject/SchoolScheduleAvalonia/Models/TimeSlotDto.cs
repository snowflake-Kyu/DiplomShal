namespace SchoolScheduleAvalonia.Models;

public sealed class TimeSlotDto
{
    public int TimeSlotId { get; set; }
    public TimeOnly? TimeStart { get; set; }
    public TimeOnly? TimeEnd { get; set; }
}
