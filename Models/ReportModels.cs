using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFPPShall.Models
{
    public class ReportModels
    {
        // Модель для отчета по расписанию
        public class ScheduleReportModel
        {
            public DateTime Date { get; set; }
            public string ClassName { get; set; }
            public string DisciplineName { get; set; }
            public string TeacherName { get; set; }
            public string AuditoriumName { get; set; }
            public TimeSpan TimeStart { get; set; }
            public TimeSpan TimeEnd { get; set; }
            public string TimeRange => $"{TimeStart:hh\\:mm} - {TimeEnd:hh\\:mm}";
        }

        // Модель для отчета по нагрузке учителей
        public class TeacherLoadReportModel
        {
            public string TeacherName { get; set; }
            public string DisciplineName { get; set; }
            public int LessonsCount { get; set; }
            public int TotalHours { get; set; }
        }

        // Модель для отчета по голосованию (оценки дисциплин)
        public class VoteReportModel
        {
            public string DisciplineName { get; set; }
            public string RateName { get; set; }
            public int VotesCount { get; set; }
        }

        // Фильтры для отчетов
        public class ScheduleFilters
        {
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public int? ClassId { get; set; }
            public int? DisciplineId { get; set; }
            public int? TeacherId { get; set; }
        }
    }
}
