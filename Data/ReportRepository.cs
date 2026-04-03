using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WPFPPShall.Models.ReportModels;

namespace WPFPPShall.Data
{
    public class ReportRepository
    {
        private readonly string _connectionString;

        public ReportRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Получение расписания с фильтрацией
        public async Task<List<ScheduleReportModel>> GetScheduleReport(ScheduleFilters filters)
        {
            var result = new List<ScheduleReportModel>();
            var query = @"
                SELECT 
                    s.Date,
                    sc.ClassName,
                    d.Name AS DisciplineName,
                    t.FullName AS TeacherName,
                    a.Name AS AuditoriumName,
                    ts.TimeStart,
                    ts.TimeEnd
                FROM Schedule s
                INNER JOIN SchoolClass sc ON s.ClassID = sc.ClassID
                INNER JOIN Discipline d ON s.DisciplineID = d.DisciplineID
                LEFT JOIN TeachingAssignment ta ON s.AssignmentID = ta.AssignmentID
                LEFT JOIN Teacher t ON ta.TeacherID = t.TeacherID
                LEFT JOIN Auditorium a ON ta.AuditoriumID = a.AuditoriumID
                LEFT JOIN TimeSlot ts ON s.TimeSlotID = ts.TimeSlotID
                WHERE 1=1";

            var parameters = new List<SqlParameter>();

            if (filters.StartDate.HasValue)
            {
                query += " AND s.Date >= @StartDate";
                parameters.Add(new SqlParameter("@StartDate", filters.StartDate.Value));
            }
            if (filters.EndDate.HasValue)
            {
                query += " AND s.Date <= @EndDate";
                parameters.Add(new SqlParameter("@EndDate", filters.EndDate.Value));
            }
            if (filters.ClassId.HasValue)
            {
                query += " AND s.ClassID = @ClassId";
                parameters.Add(new SqlParameter("@ClassId", filters.ClassId.Value));
            }
            if (filters.DisciplineId.HasValue)
            {
                query += " AND s.DisciplineID = @DisciplineId";
                parameters.Add(new SqlParameter("@DisciplineId", filters.DisciplineId.Value));
            }
            if (filters.TeacherId.HasValue)
            {
                query += " AND t.TeacherID = @TeacherId";
                parameters.Add(new SqlParameter("@TeacherId", filters.TeacherId.Value));
            }

            query += " ORDER BY s.Date, ts.TimeStart";

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters.ToArray());
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new ScheduleReportModel
                            {
                                Date = reader.GetDateTime(0),
                                ClassName = reader.GetString(1),
                                DisciplineName = reader.GetString(2),
                                TeacherName = reader.IsDBNull(3) ? "Не назначен" : reader.GetString(3),
                                AuditoriumName = reader.IsDBNull(4) ? "Не указан" : reader.GetString(4),
                                TimeStart = reader.GetTimeSpan(5),
                                TimeEnd = reader.GetTimeSpan(6)
                            });
                        }
                    }
                }
            }
            return result;
        }

        // Получение нагрузки учителей
        public async Task<List<TeacherLoadReportModel>> GetTeacherLoadReport(int? teacherId = null, int? classId = null)
        {
            var result = new List<TeacherLoadReportModel>();
            var query = @"
                SELECT 
                    t.FullName AS TeacherName,
                    d.Name AS DisciplineName,
                    COUNT(s.ScheduleID) AS LessonsCount,
                    COUNT(s.ScheduleID) * 1 AS TotalHours
                FROM Schedule s
                INNER JOIN TeachingAssignment ta ON s.AssignmentID = ta.AssignmentID
                INNER JOIN Teacher t ON ta.TeacherID = t.TeacherID
                INNER JOIN Discipline d ON s.DisciplineID = d.DisciplineID
                WHERE 1=1";

            var parameters = new List<SqlParameter>();

            if (teacherId.HasValue)
            {
                query += " AND t.TeacherID = @TeacherId";
                parameters.Add(new SqlParameter("@TeacherId", teacherId.Value));
            }
            if (classId.HasValue)
            {
                query += " AND s.ClassID = @ClassId";
                parameters.Add(new SqlParameter("@ClassId", classId.Value));
            }

            query += @" GROUP BY t.FullName, d.Name 
                        ORDER BY t.FullName, LessonsCount DESC";

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters.ToArray());
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new TeacherLoadReportModel
                            {
                                TeacherName = reader.GetString(0),
                                DisciplineName = reader.GetString(1),
                                LessonsCount = reader.GetInt32(2),
                                TotalHours = reader.GetInt32(3)
                            });
                        }
                    }
                }
            }
            return result;
        }

        // Получение списка классов для фильтра
        public async Task<DataTable> GetClasses()
        {
            var dt = new DataTable();
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var adapter = new SqlDataAdapter("SELECT ClassID, ClassName FROM SchoolClass ORDER BY ClassName", connection))
                {
                    await Task.Run(() => adapter.Fill(dt));
                }
            }
            return dt;
        }

        // Получение списка дисциплин
        public async Task<DataTable> GetDisciplines()
        {
            var dt = new DataTable();
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var adapter = new SqlDataAdapter("SELECT DisciplineID, Name FROM Discipline ORDER BY Name", connection))
                {
                    await Task.Run(() => adapter.Fill(dt));
                }
            }
            return dt;
        }

        // Получение списка учителей
        public async Task<DataTable> GetTeachers()
        {
            var dt = new DataTable();
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var adapter = new SqlDataAdapter("SELECT TeacherID, FullName FROM Teacher ORDER BY FullName", connection))
                {
                    await Task.Run(() => adapter.Fill(dt));
                }
            }
            return dt;
        }
    }
}
