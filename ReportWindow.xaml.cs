using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using OfficeOpenXml;
using WPFPPShall.Data;
using static WPFPPShall.Models.ReportModels;

namespace WPFPPShall
{
    /// <summary>
    /// Логика взаимодействия для ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {
        private readonly ReportRepository _repository;
        private DataTable _currentReportData;

        public ReportWindow()
        {
            InitializeComponent();

            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=KP_2024_Shalamov;Integrated Security=True;";
            _repository = new ReportRepository(connectionString);

            Loaded += async (s, e) => await LoadFilters();
        }

        private async Task LoadFilters()
        {
            try
            {
                // Загрузка данных для фильтров
                var classes = await _repository.GetClasses();
                cbClass.ItemsSource = classes.DefaultView;
                cbClass.SelectedValuePath = "ClassID";
                cbClass.DisplayMemberPath = "ClassName";

                // Добавляем "Все классы"
                var allClasses = classes.Clone();
                allClasses.Rows.Add(-1, "-- Все классы --");
                foreach (DataRow row in classes.Rows)
                    allClasses.ImportRow(row);
                cbClass.ItemsSource = allClasses.DefaultView;
                cbClass.SelectedIndex = 0;

                var disciplines = await _repository.GetDisciplines();
                var allDisciplines = disciplines.Clone();
                allDisciplines.Rows.Add(-1, "-- Все дисциплины --");
                foreach (DataRow row in disciplines.Rows)
                    allDisciplines.ImportRow(row);
                cbDiscipline.ItemsSource = allDisciplines.DefaultView;
                cbDiscipline.SelectedIndex = 0;

                var teachers = await _repository.GetTeachers();
                var allTeachers = teachers.Clone();
                allTeachers.Rows.Add(-1, "-- Все учителя --");
                foreach (DataRow row in teachers.Rows)
                    allTeachers.ImportRow(row);
                cbTeacher.ItemsSource = allTeachers.DefaultView;
                cbTeacher.SelectedIndex = 0;

                // Устанавливаем даты по умолчанию
                dpStartDate.SelectedDate = DateTime.Today.AddDays(-30);
                dpEndDate.SelectedDate = DateTime.Today;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фильтров: {ex.Message}");
            }
        }

        private async void ReportTypeChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                // Защита от null
                if (rbSchedule == null || rbTeacherLoad == null || rbVotes == null)
                {
                    MessageBox.Show("Ошибка: RadioButton не инициализированы");
                    return;
                }

                bool isSchedule = rbSchedule.IsChecked.GetValueOrDefault(false);
                bool isTeacherLoad = rbTeacherLoad.IsChecked.GetValueOrDefault(false);

                if (filterDatePanel != null)
                    filterDatePanel.Visibility = isSchedule ? Visibility.Visible : Visibility.Collapsed;

                if (filterTeacherPanel != null)
                    filterTeacherPanel.Visibility = isSchedule || isTeacherLoad ? Visibility.Visible : Visibility.Collapsed;

                if (filterClassPanel != null)
                    filterClassPanel.Visibility = isSchedule || isTeacherLoad ? Visibility.Visible : Visibility.Collapsed;

                if (filterDisciplinePanel != null)
                    filterDisciplinePanel.Visibility = isSchedule ? Visibility.Visible : Visibility.Collapsed;

                // Не вызываем LoadReport автоматически
                // await LoadReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private async void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            await LoadReport();
        }

        private async Task LoadReport()
        {
            try
            {
                tbStatus.Text = "Загрузка данных...";
                Cursor = System.Windows.Input.Cursors.Wait;

                if (rbSchedule.IsChecked == true)
                {
                    await LoadScheduleReport();
                }
                else if (rbTeacherLoad.IsChecked == true)
                {
                    await LoadTeacherLoadReport();
                }
                else if (rbVotes.IsChecked == true)
                {
                    await LoadVotesReport();
                }

                tbStatus.Text = $"Загружено строк: {dgReport.Items.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отчета: {ex.Message}");
                tbStatus.Text = "Ошибка загрузки";
            }
            finally
            {
                Cursor = null;
            }
        }

        private async Task LoadScheduleReport()
        {
            int classId = GetSelectedId(cbClass);
            int disciplineId = GetSelectedId(cbDiscipline);
            int teacherId = GetSelectedId(cbTeacher);

            var filters = new ScheduleFilters
            {
                StartDate = dpStartDate.SelectedDate,
                EndDate = dpEndDate.SelectedDate,
                ClassId = classId == -1 ? null : (int?)classId,
                DisciplineId = disciplineId == -1 ? null : (int?)disciplineId,
                TeacherId = teacherId == -1 ? null : (int?)teacherId
            };

            var report = await _repository.GetScheduleReport(filters);
            dgReport.ItemsSource = report;
        }

        private async Task LoadTeacherLoadReport()
        {
            int? teacherId = null;
            int? classId = null;

            int selectedTeacher = GetSelectedId(cbTeacher);
            int selectedClass = GetSelectedId(cbClass);

            if (selectedTeacher != -1)
                teacherId = selectedTeacher;

            if (selectedClass != -1)
                classId = selectedClass;

            var report = await _repository.GetTeacherLoadReport(teacherId, classId);

            dgReport.ItemsSource = report;
        }

        private async Task LoadVotesReport()
        {
            // Запрос для отчета по голосованию
            var query = @"
                SELECT 
                    d.Name AS Дисциплина,
                    r.RateName AS Оценка,
                    COUNT(v.VoteID) AS Количество_голосов
                FROM Vote v
                INNER JOIN Discipline d ON v.DisciplineID = d.DisciplineID
                INNER JOIN Rate r ON v.RateID = r.RateID
                GROUP BY d.Name, r.RateName
                ORDER BY d.Name, 
                    CASE r.RateName 
                        WHEN 'Легко' THEN 1 
                        WHEN 'Нормально' THEN 2 
                        WHEN 'Сложно' THEN 3 
                    END";

            using (var connection = new System.Data.SqlClient.SqlConnection(
                "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=KP_2024_Shalamov;Integrated Security=True;"))
            {
                using (var adapter = new System.Data.SqlClient.SqlDataAdapter(query, connection))
                {
                    var dt = new DataTable();
                    await Task.Run(() => adapter.Fill(dt));
                    dgReport.ItemsSource = dt.DefaultView;
                }
            }
        }

        private int GetSelectedId(ComboBox combo)
        {
            if (combo.SelectedValue == null)
                return -1;

            // Если SelectedValue это DataRowView
            if (combo.SelectedValue is DataRowView rowView)
            {
                var val = rowView[combo.SelectedValuePath];
                if (val is int)
                    return (int)val;
                if (val is string && int.TryParse((string)val, out int result))
                    return result;
                return -1;
            }

            // Если SelectedValue это int
            if (combo.SelectedValue is int)
                return (int)combo.SelectedValue;

            // Если SelectedValue это string
            if (combo.SelectedValue is string str && int.TryParse(str, out int intResult))
                return intResult;

            return -1;
        }

        private async void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            cbClass.SelectedIndex = 0;
            cbDiscipline.SelectedIndex = 0;
            cbTeacher.SelectedIndex = 0;
            dpStartDate.SelectedDate = DateTime.Today.AddDays(-30);
            dpEndDate.SelectedDate = DateTime.Today;

            await LoadReport();
        }

        [Obsolete]
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgReport.Items.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта");
                    return;
                }

                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    DefaultExt = "xlsx",
                    FileName = $"Отчет_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    ExportToExcel(saveDialog.FileName);
                    MessageBox.Show($"Отчет сохранен в:\n{saveDialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}");
            }
        }

        [Obsolete]
        private void ExportToExcel(string filePath)
        {
            try
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Отчет");

                    // Получаем данные из ItemsSource
                    var itemsSource = dgReport.ItemsSource;
                    if (itemsSource == null) return;

                    // Заголовки
                    for (int i = 0; i < dgReport.Columns.Count; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = dgReport.Columns[i].Header;
                        worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    }

                    // Данные
                    var itemsList = new List<object>();
                    foreach (var item in itemsSource)
                        itemsList.Add(item);

                    for (int row = 0; row < itemsList.Count; row++)
                    {
                        var item = itemsList[row];
                        for (int col = 0; col < dgReport.Columns.Count; col++)
                        {
                            var column = dgReport.Columns[col];

                            // Получаем значение свойства
                            var property = item.GetType().GetProperty(column.SortMemberPath);
                            if (property != null)
                            {
                                var value = property.GetValue(item);
                                worksheet.Cells[row + 2, col + 1].Value = value?.ToString() ?? "";
                            }
                            else
                            {
                                // Fallback через TextBlock
                                var cellContent = column.GetCellContent(item);
                                var textBlock = cellContent as TextBlock;
                                worksheet.Cells[row + 2, col + 1].Value = textBlock?.Text ?? "";
                            }
                        }
                    }

                    worksheet.Cells.AutoFitColumns();
                    package.SaveAs(new System.IO.FileInfo(filePath));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}");
                throw;
            }
        }
    }
}
