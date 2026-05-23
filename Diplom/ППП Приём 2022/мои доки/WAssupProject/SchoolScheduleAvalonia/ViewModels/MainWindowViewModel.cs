using System.Collections.ObjectModel;
using SchoolScheduleAvalonia.Models;
using SchoolScheduleAvalonia.Services;

namespace SchoolScheduleAvalonia.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private readonly ApiClient _apiClient;
    private readonly List<ScheduleRow> _allRows = new();

    private ScheduleRow? _selectedRow;
    private string _searchText = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isBusy;
    private int _selectedScheduleId;
    private DateTimeOffset? _selectedDate;
    private LookupItem? _selectedClass;
    private LookupItem? _selectedDiscipline;
    private LookupItem? _selectedTimeSlot;
    private LookupItem? _selectedAssignment;

    public ObservableCollection<ScheduleRow> ScheduleRows { get; } = new();
    public ObservableCollection<LookupItem> ClassOptions { get; } = new();
    public ObservableCollection<LookupItem> DisciplineOptions { get; } = new();
    public ObservableCollection<LookupItem> TimeSlotOptions { get; } = new();
    public ObservableCollection<LookupItem> AssignmentOptions { get; } = new();

    public AsyncCommand RefreshCommand { get; }
    public AsyncCommand SaveCommand { get; }
    public AsyncCommand DeleteCommand { get; }
    public AsyncCommand ClearFormCommand { get; }

    public string WindowTitle => "Расписание школы, Avalonia UI";
    public string ApiBaseUrl => _apiClient.BaseUrl;

    public MainWindowViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;

        RefreshCommand = new AsyncCommand(LoadAsync, () => !IsBusy);
        SaveCommand = new AsyncCommand(SaveAsync, () => !IsBusy);
        DeleteCommand = new AsyncCommand(DeleteAsync, () => !IsBusy && SelectedScheduleId > 0);
        ClearFormCommand = new AsyncCommand(ClearFormAsync, () => !IsBusy);

        _ = LoadAsync();
    }

    public ScheduleRow? SelectedRow
    {
        get => _selectedRow;
        set
        {
            if (SetProperty(ref _selectedRow, value))
            {
                FillFormFromSelection(value);
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                ApplyFilter();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
                SaveCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
                ClearFormCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public int SelectedScheduleId
    {
        get => _selectedScheduleId;
        set
        {
            if (SetProperty(ref _selectedScheduleId, value))
            {
                DeleteCommand.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(FormModeText));
            }
        }
    }

    public string FormModeText => SelectedScheduleId > 0
        ? $"Редактирование записи №{SelectedScheduleId}"
        : "Создание новой записи";

    public DateTimeOffset? SelectedDate
    {
        get => _selectedDate;
        set => SetProperty(ref _selectedDate, value);
    }

    public LookupItem? SelectedClass
    {
        get => _selectedClass;
        set => SetProperty(ref _selectedClass, value);
    }

    public LookupItem? SelectedDiscipline
    {
        get => _selectedDiscipline;
        set => SetProperty(ref _selectedDiscipline, value);
    }

    public LookupItem? SelectedTimeSlot
    {
        get => _selectedTimeSlot;
        set => SetProperty(ref _selectedTimeSlot, value);
    }

    public LookupItem? SelectedAssignment
    {
        get => _selectedAssignment;
        set => SetProperty(ref _selectedAssignment, value);
    }

    private async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Загрузка данных из API...";

            var schedulesTask = _apiClient.GetListAsync<ScheduleDto>("schedules");
            var classesTask = _apiClient.GetListAsync<SchoolClassDto>("schoolclasses");
            var disciplinesTask = _apiClient.GetListAsync<DisciplineDto>("disciplines");
            var timeSlotsTask = _apiClient.GetListAsync<TimeSlotDto>("timeslots");
            var assignmentsTask = _apiClient.GetListAsync<TeachingAssignmentDto>("teachingassignments");
            var teachersTask = _apiClient.GetListAsync<TeacherDto>("teachers");
            var auditoriumsTask = _apiClient.GetListAsync<AuditoriumDto>("auditoriums");

            await Task.WhenAll(
                schedulesTask,
                classesTask,
                disciplinesTask,
                timeSlotsTask,
                assignmentsTask,
                teachersTask,
                auditoriumsTask);

            var schedules = schedulesTask.Result;
            var classes = classesTask.Result;
            var disciplines = disciplinesTask.Result;
            var timeSlots = timeSlotsTask.Result;
            var assignments = assignmentsTask.Result;
            var teachers = teachersTask.Result;
            var auditoriums = auditoriumsTask.Result;

            FillLookupCollections(classes, disciplines, timeSlots, assignments, teachers, auditoriums);
            BuildScheduleRows(schedules, classes, disciplines, timeSlots, assignments, teachers, auditoriums);
            ApplyFilter();

            StatusMessage = $"Загружено записей расписания: {_allRows.Count}. API: {ApiBaseUrl}";
        }
        catch (Exception ex)
        {
            StatusMessage = "Ошибка загрузки: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void FillLookupCollections(
        List<SchoolClassDto> classes,
        List<DisciplineDto> disciplines,
        List<TimeSlotDto> timeSlots,
        List<TeachingAssignmentDto> assignments,
        List<TeacherDto> teachers,
        List<AuditoriumDto> auditoriums)
    {
        ClassOptions.Clear();
        foreach (var item in classes.OrderBy(x => x.ClassName))
        {
            ClassOptions.Add(new LookupItem
            {
                Id = item.ClassId,
                Name = string.IsNullOrWhiteSpace(item.ClassName) ? $"Класс #{item.ClassId}" : item.ClassName!
            });
        }

        DisciplineOptions.Clear();
        foreach (var item in disciplines.OrderBy(x => x.Name))
        {
            DisciplineOptions.Add(new LookupItem
            {
                Id = item.DisciplineId,
                Name = string.IsNullOrWhiteSpace(item.Name) ? $"Дисциплина #{item.DisciplineId}" : item.Name!
            });
        }

        TimeSlotOptions.Clear();
        foreach (var item in timeSlots.OrderBy(x => x.TimeStart))
        {
            TimeSlotOptions.Add(new LookupItem
            {
                Id = item.TimeSlotId,
                Name = BuildTimeSlotText(item)
            });
        }

        AssignmentOptions.Clear();
        foreach (var item in assignments.OrderBy(x => x.AssignmentId))
        {
            AssignmentOptions.Add(new LookupItem
            {
                Id = item.AssignmentId,
                Name = BuildAssignmentText(item, teachers, disciplines, auditoriums)
            });
        }
    }

    private void BuildScheduleRows(
        List<ScheduleDto> schedules,
        List<SchoolClassDto> classes,
        List<DisciplineDto> disciplines,
        List<TimeSlotDto> timeSlots,
        List<TeachingAssignmentDto> assignments,
        List<TeacherDto> teachers,
        List<AuditoriumDto> auditoriums)
    {
        _allRows.Clear();

        foreach (var schedule in schedules
            .OrderBy(x => x.Date)
            .ThenBy(x => x.TimeSlotId)
            .ThenBy(x => x.ClassId))
        {
            var assignment = assignments.FirstOrDefault(x => x.AssignmentId == schedule.AssignmentId);
            var teacher = assignment == null ? null : teachers.FirstOrDefault(x => x.TeacherId == assignment.TeacherId);
            var auditorium = assignment?.AuditoriumId == null
                ? null
                : auditoriums.FirstOrDefault(x => x.AuditoriumId == assignment.AuditoriumId);

            var row = new ScheduleRow
            {
                Schedule = schedule,
                DateText = schedule.Date?.ToString("dd.MM.yyyy") ?? "не указана",
                ClassName = classes.FirstOrDefault(x => x.ClassId == schedule.ClassId)?.ClassName ?? EmptyValue(schedule.ClassId, "класс"),
                DisciplineName = disciplines.FirstOrDefault(x => x.DisciplineId == schedule.DisciplineId)?.Name ?? EmptyValue(schedule.DisciplineId, "дисциплина"),
                TimeSlotText = BuildTimeSlotText(timeSlots.FirstOrDefault(x => x.TimeSlotId == schedule.TimeSlotId)),
                TeacherName = teacher?.FullName ?? EmptyValue(assignment?.TeacherId, "преподаватель"),
                AuditoriumName = auditorium?.Name ?? EmptyValue(assignment?.AuditoriumId, "аудитория"),
                AssignmentText = assignment == null ? EmptyValue(schedule.AssignmentId, "назначение") : BuildAssignmentText(assignment, teachers, disciplines, auditoriums)
            };

            _allRows.Add(row);
        }
    }

    private void ApplyFilter()
    {
        var query = SearchText.Trim();
        var filtered = string.IsNullOrWhiteSpace(query)
            ? _allRows
            : _allRows.Where(row =>
                Contains(row.DateText, query) ||
                Contains(row.ClassName, query) ||
                Contains(row.DisciplineName, query) ||
                Contains(row.TimeSlotText, query) ||
                Contains(row.TeacherName, query) ||
                Contains(row.AuditoriumName, query) ||
                Contains(row.AssignmentText, query));

        ScheduleRows.Clear();
        foreach (var row in filtered)
        {
            ScheduleRows.Add(row);
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Сохранение записи расписания...";

            var schedule = new ScheduleDto
            {
                ScheduleId = SelectedScheduleId,
                Date = SelectedDate.HasValue ? DateOnly.FromDateTime(SelectedDate.Value.DateTime) : null,
                ClassId = SelectedClass?.Id,
                DisciplineId = SelectedDiscipline?.Id,
                TimeSlotId = SelectedTimeSlot?.Id,
                AssignmentId = SelectedAssignment?.Id
            };

            if (schedule.ScheduleId <= 0)
            {
                await _apiClient.PostAsync("schedules", schedule);
                StatusMessage = "Запись добавлена.";
            }
            else
            {
                await _apiClient.PutAsync($"schedules/{schedule.ScheduleId}", schedule);
                StatusMessage = $"Запись №{schedule.ScheduleId} обновлена.";
            }

            await LoadAsync();
            await ClearFormAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = "Ошибка сохранения: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteAsync()
    {
        if (SelectedScheduleId <= 0)
        {
            StatusMessage = "Для удаления выбери запись расписания.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = $"Удаление записи №{SelectedScheduleId}...";

            await _apiClient.DeleteAsync($"schedules/{SelectedScheduleId}");
            StatusMessage = $"Запись №{SelectedScheduleId} удалена.";

            await LoadAsync();
            await ClearFormAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = "Ошибка удаления: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private Task ClearFormAsync()
    {
        SelectedRow = null;
        SelectedScheduleId = 0;
        SelectedDate = null;
        SelectedClass = null;
        SelectedDiscipline = null;
        SelectedTimeSlot = null;
        SelectedAssignment = null;
        StatusMessage = "Форма очищена. Можно создать новую запись.";
        return Task.CompletedTask;
    }

    private void FillFormFromSelection(ScheduleRow? row)
    {
        if (row == null)
        {
            return;
        }

        var schedule = row.Schedule;
        SelectedScheduleId = schedule.ScheduleId;
        SelectedDate = schedule.Date.HasValue
            ? new DateTimeOffset(schedule.Date.Value.ToDateTime(TimeOnly.MinValue))
            : null;
        SelectedClass = ClassOptions.FirstOrDefault(x => x.Id == schedule.ClassId);
        SelectedDiscipline = DisciplineOptions.FirstOrDefault(x => x.Id == schedule.DisciplineId);
        SelectedTimeSlot = TimeSlotOptions.FirstOrDefault(x => x.Id == schedule.TimeSlotId);
        SelectedAssignment = AssignmentOptions.FirstOrDefault(x => x.Id == schedule.AssignmentId);
        StatusMessage = $"Выбрана запись расписания №{schedule.ScheduleId}.";
    }

    private static string BuildTimeSlotText(TimeSlotDto? timeSlot)
    {
        if (timeSlot == null)
        {
            return "время не указано";
        }

        var start = timeSlot.TimeStart?.ToString("HH:mm") ?? "??:??";
        var end = timeSlot.TimeEnd?.ToString("HH:mm") ?? "??:??";
        return $"{start} - {end}";
    }

    private static string BuildAssignmentText(
        TeachingAssignmentDto assignment,
        List<TeacherDto> teachers,
        List<DisciplineDto> disciplines,
        List<AuditoriumDto> auditoriums)
    {
        var teacher = teachers.FirstOrDefault(x => x.TeacherId == assignment.TeacherId)?.FullName ?? $"преп. #{assignment.TeacherId}";
        var discipline = disciplines.FirstOrDefault(x => x.DisciplineId == assignment.DisciplineId)?.Name ?? $"дисц. #{assignment.DisciplineId}";
        var auditorium = assignment.AuditoriumId.HasValue
            ? auditoriums.FirstOrDefault(x => x.AuditoriumId == assignment.AuditoriumId.Value)?.Name ?? $"ауд. #{assignment.AuditoriumId.Value}"
            : "ауд. не указана";

        return $"#{assignment.AssignmentId}: {teacher}, {discipline}, {auditorium}";
    }

    private static string EmptyValue(int? id, string entityName)
    {
        return id.HasValue ? $"{entityName} #{id.Value}" : "не указано";
    }

    private static bool Contains(string source, string query)
    {
        return source.Contains(query, StringComparison.OrdinalIgnoreCase);
    }
}
