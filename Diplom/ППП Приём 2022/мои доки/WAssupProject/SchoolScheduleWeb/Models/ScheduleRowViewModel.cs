namespace SchoolScheduleWeb.Models;

public class ScheduleRowViewModel
{
    public int ScheduleId { get; set; }
    public DateOnly? Date { get; set; }
    public string DateText => Date?.ToString("dd.MM.yyyy") ?? "—";
    public string DayOfWeekText => Date.HasValue ? GetRussianDayName(Date.Value.DayOfWeek) : "—";

    public int? ClassId { get; set; }
    public string ClassName { get; set; } = "—";

    public int? DisciplineId { get; set; }
    public string DisciplineName { get; set; } = "—";

    public int? TimeSlotId { get; set; }
    public string TimeText { get; set; } = "—";

    public int? AssignmentId { get; set; }
    public int? TeacherId { get; set; }
    public string TeacherName { get; set; } = "—";
    public string AuditoriumName { get; set; } = "—";
    public string AssignmentComment { get; set; } = "";

    private static string GetRussianDayName(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => "Понедельник",
            DayOfWeek.Tuesday => "Вторник",
            DayOfWeek.Wednesday => "Среда",
            DayOfWeek.Thursday => "Четверг",
            DayOfWeek.Friday => "Пятница",
            DayOfWeek.Saturday => "Суббота",
            DayOfWeek.Sunday => "Воскресенье",
            _ => "—"
        };
    }
}
