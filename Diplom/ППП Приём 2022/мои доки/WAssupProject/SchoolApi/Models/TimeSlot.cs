namespace SchoolApi.Models;

public class TimeSlot
{
    public int TimeSlotId { get; set; }
    public TimeOnly? TimeStart { get; set; }
    public TimeOnly? TimeEnd { get; set; }
}
