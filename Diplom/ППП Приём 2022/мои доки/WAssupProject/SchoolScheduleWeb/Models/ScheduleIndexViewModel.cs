namespace SchoolScheduleWeb.Models;

public class ScheduleIndexViewModel
{
    public List<ScheduleRowViewModel> Rows { get; set; } = new();
    public List<SchoolClass> Classes { get; set; } = new();
    public List<Discipline> Disciplines { get; set; } = new();
    public List<Teacher> Teachers { get; set; } = new();

    public string? Date { get; set; }
    public int? ClassId { get; set; }
    public int? DisciplineId { get; set; }
    public int? TeacherId { get; set; }
    public string? Search { get; set; }

    public string? ErrorMessage { get; set; }
    public string ApiBaseUrl { get; set; } = "";
}
