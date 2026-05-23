namespace SchoolScheduleAvalonia.Models;

public sealed class ScheduleRow
{
    public ScheduleDto Schedule { get; set; } = new();

    public int ScheduleId => Schedule.ScheduleId;
    public string DateText { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string DisciplineName { get; set; } = string.Empty;
    public string TimeSlotText { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string AuditoriumName { get; set; } = string.Empty;
    public string AssignmentText { get; set; } = string.Empty;
}
