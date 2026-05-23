using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using SchoolScheduleWeb.Models;
using SchoolScheduleWeb.Services;

namespace SchoolScheduleWeb.Controllers;

public class ScheduleController : Controller
{
    private readonly ApiClient _apiClient;

    public ScheduleController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(string? date, int? classId, int? disciplineId, int? teacherId, string? search, CancellationToken cancellationToken)
    {
        var model = new ScheduleIndexViewModel
        {
            Date = date,
            ClassId = classId,
            DisciplineId = disciplineId,
            TeacherId = teacherId,
            Search = search,
            ApiBaseUrl = _apiClient.BaseUrl
        };

        try
        {
            var rows = await BuildScheduleRowsAsync(cancellationToken);

            model.Classes = await _apiClient.GetListAsync<SchoolClass>("schoolclasses", cancellationToken);
            model.Disciplines = await _apiClient.GetListAsync<Discipline>("disciplines", cancellationToken);
            model.Teachers = await _apiClient.GetListAsync<Teacher>("teachers", cancellationToken);

            rows = ApplyFilters(rows, date, classId, disciplineId, teacherId, search);
            model.Rows = rows.OrderBy(r => r.Date).ThenBy(r => r.TimeText).ThenBy(r => r.ClassName).ToList();
        }
        catch (InvalidOperationException ex)
        {
            model.ErrorMessage = ex.Message;
        }

        return View(model);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var model = new ScheduleDetailsViewModel
        {
            ApiBaseUrl = _apiClient.BaseUrl
        };

        try
        {
            var rows = await BuildScheduleRowsAsync(cancellationToken);
            model.Row = rows.FirstOrDefault(x => x.ScheduleId == id);

            if (model.Row == null)
            {
                model.ErrorMessage = $"Запись расписания с ID {id} не найдена.";
            }
        }
        catch (InvalidOperationException ex)
        {
            model.ErrorMessage = ex.Message;
        }

        return View(model);
    }

    private async Task<List<ScheduleRowViewModel>> BuildScheduleRowsAsync(CancellationToken cancellationToken)
    {
        var schedulesTask = _apiClient.GetListAsync<Schedule>("schedules", cancellationToken);
        var classesTask = _apiClient.GetListAsync<SchoolClass>("schoolclasses", cancellationToken);
        var disciplinesTask = _apiClient.GetListAsync<Discipline>("disciplines", cancellationToken);
        var timeSlotsTask = _apiClient.GetListAsync<TimeSlot>("timeslots", cancellationToken);
        var assignmentsTask = _apiClient.GetListAsync<TeachingAssignment>("teachingassignments", cancellationToken);
        var teachersTask = _apiClient.GetListAsync<Teacher>("teachers", cancellationToken);
        var auditoriumsTask = _apiClient.GetListAsync<Auditorium>("auditoriums", cancellationToken);

        await Task.WhenAll(schedulesTask, classesTask, disciplinesTask, timeSlotsTask, assignmentsTask, teachersTask, auditoriumsTask);

        var schedules = schedulesTask.Result;
        var classes = classesTask.Result.ToDictionary(x => x.ClassId);
        var disciplines = disciplinesTask.Result.ToDictionary(x => x.DisciplineId);
        var timeSlots = timeSlotsTask.Result.ToDictionary(x => x.TimeSlotId);
        var assignments = assignmentsTask.Result.ToDictionary(x => x.AssignmentId);
        var teachers = teachersTask.Result.ToDictionary(x => x.TeacherId);
        var auditoriums = auditoriumsTask.Result.ToDictionary(x => x.AuditoriumId);

        return schedules.Select(schedule =>
        {
            classes.TryGetValue(schedule.ClassId ?? 0, out var schoolClass);
            disciplines.TryGetValue(schedule.DisciplineId ?? 0, out var discipline);
            timeSlots.TryGetValue(schedule.TimeSlotId ?? 0, out var timeSlot);
            assignments.TryGetValue(schedule.AssignmentId ?? 0, out var assignment);

            Teacher? teacher = null;
            Auditorium? auditorium = null;

            if (assignment != null)
            {
                teachers.TryGetValue(assignment.TeacherId, out teacher);
                if (assignment.AuditoriumId.HasValue)
                {
                    auditoriums.TryGetValue(assignment.AuditoriumId.Value, out auditorium);
                }
            }

            return new ScheduleRowViewModel
            {
                ScheduleId = schedule.ScheduleId,
                Date = schedule.Date,
                ClassId = schedule.ClassId,
                ClassName = schoolClass?.ClassName ?? $"Класс ID {schedule.ClassId}",
                DisciplineId = schedule.DisciplineId,
                DisciplineName = discipline?.Name ?? $"Дисциплина ID {schedule.DisciplineId}",
                TimeSlotId = schedule.TimeSlotId,
                TimeText = FormatTimeSlot(timeSlot),
                AssignmentId = schedule.AssignmentId,
                TeacherId = teacher?.TeacherId,
                TeacherName = teacher?.FullName ?? "—",
                AuditoriumName = auditorium?.Name ?? "—",
                AssignmentComment = assignment?.Comment ?? string.Empty
            };
        }).ToList();
    }

    private static List<ScheduleRowViewModel> ApplyFilters(List<ScheduleRowViewModel> rows, string? date, int? classId, int? disciplineId, int? teacherId, string? search)
    {
        if (!string.IsNullOrWhiteSpace(date) && DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            rows = rows.Where(x => x.Date == parsedDate).ToList();
        }

        if (classId.HasValue)
        {
            rows = rows.Where(x => x.ClassId == classId.Value).ToList();
        }

        if (disciplineId.HasValue)
        {
            rows = rows.Where(x => x.DisciplineId == disciplineId.Value).ToList();
        }

        if (teacherId.HasValue)
        {
            rows = rows.Where(x => x.TeacherId == teacherId.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLowerInvariant();
            rows = rows.Where(x =>
                x.ClassName.ToLowerInvariant().Contains(normalizedSearch) ||
                x.DisciplineName.ToLowerInvariant().Contains(normalizedSearch) ||
                x.TeacherName.ToLowerInvariant().Contains(normalizedSearch) ||
                x.AuditoriumName.ToLowerInvariant().Contains(normalizedSearch) ||
                x.TimeText.ToLowerInvariant().Contains(normalizedSearch) ||
                x.DayOfWeekText.ToLowerInvariant().Contains(normalizedSearch)
            ).ToList();
        }

        return rows;
    }

    private static string FormatTimeSlot(TimeSlot? slot)
    {
        if (slot == null)
        {
            return "—";
        }

        var start = slot.TimeStart?.ToString("HH:mm") ?? "?";
        var end = slot.TimeEnd?.ToString("HH:mm") ?? "?";
        return $"{start}–{end}";
    }
}
