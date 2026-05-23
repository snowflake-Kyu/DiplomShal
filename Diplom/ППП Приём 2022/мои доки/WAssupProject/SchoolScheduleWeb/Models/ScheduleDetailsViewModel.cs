namespace SchoolScheduleWeb.Models;

public class ScheduleDetailsViewModel
{
    public ScheduleRowViewModel? Row { get; set; }
    public string? ErrorMessage { get; set; }
    public string ApiBaseUrl { get; set; } = "";
}
