using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

namespace WPFPPShall
{
    public partial class FormGenerateSchedule : Window
    {
        private string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=KP_2024_Shalamov;Integrated Security=True;";
        private DataTable allTeachers;
        private bool isDarkTheme = true;

        public FormGenerateSchedule()
        {
            InitializeComponent();
            LoadTeachers();
            ApplyTheme(true);
            ThemeToggle.IsChecked = true;
        }

        private void LoadTeachers()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        t.TeacherID, 
                        t.FullName,
                        STUFF((SELECT ', ' + d.Name
                              FROM TeachingAssignment ta 
                              JOIN Discipline d ON ta.DisciplineID = d.DisciplineID
                              WHERE ta.TeacherID = t.TeacherID
                              FOR XML PATH('')), 1, 2, '') as Discipline
                    FROM Teacher t
                    ORDER BY t.FullName", conn);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                allTeachers = new DataTable();
                da.Fill(allTeachers);

                // Добавляем колонку для статуса занятости
                allTeachers.Columns.Add("IsBusy", typeof(bool));

                // Инициализируем все как false (не заняты)
                foreach (DataRow row in allTeachers.Rows)
                {
                    row["IsBusy"] = false;
                }

                TeacherList.ItemsSource = allTeachers.DefaultView;
                UpdateCounters();

                // Подписываемся на событие выбора
                TeacherList.SelectionChanged += TeacherList_SelectionChanged;
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (allTeachers == null) return;

            string searchText = SearchTextBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                allTeachers.DefaultView.RowFilter = string.Empty;
            }
            else
            {
                allTeachers.DefaultView.RowFilter = $"FullName LIKE '%{searchText}%' OR Discipline LIKE '%{searchText}%'";
            }

            UpdateCounters();
        }

        // Выбрать всех учителей
        private void BtnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            TeacherList.SelectAll();
            UpdateCounters();
        }

        // Очистить выбор всех учителей
        private void BtnClearAll_Click(object sender, RoutedEventArgs e)
        {
            TeacherList.UnselectAll();
            UpdateCounters();
        }

        // Обновление счетчиков - ИСПРАВЛЕНО!
        private void UpdateCounters()
        {
            int selected = TeacherList.SelectedItems.Count;
            int total = allTeachers?.Rows.Count ?? 0;

            SelectedCountText.Text = selected.ToString();
            TotalCountText.Text = total.ToString();

            StatusText.Text = selected > 0
                ? $"Готово к генерации ({selected} преподавателей)"
                : "Ожидание выбора";

            // Изменяем цвет статуса с использованием текущей темы
            if (selected > 0)
            {
                var successColor = (SolidColorBrush)Application.Current.Resources["SuccessColor"];
                StatusText.Foreground = successColor;
            }
            else
            {
                var textSecondary = (SolidColorBrush)Application.Current.Resources["TextSecondary"];
                StatusText.Foreground = textSecondary;
            }
        }

        // Обработчик изменения выбора в списке
        private void TeacherList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCounters();
        }

        // === ТЕМЫ ===

        private void ApplyTheme(bool darkTheme)
        {
            isDarkTheme = darkTheme;

            if (darkTheme)
            {
                Application.Current.Resources["BackgroundColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                Application.Current.Resources["CardBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#252526"));
                Application.Current.Resources["BorderColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E3E42"));
                Application.Current.Resources["TextColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
                Application.Current.Resources["TextSecondary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A0A0A0"));
                Application.Current.Resources["AccentColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4A6FA5"));
                Application.Current.Resources["AccentHover"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A5A95"));
                Application.Current.Resources["SuccessColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
            }
            else
            {
                Application.Current.Resources["BackgroundColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8F9FA"));
                Application.Current.Resources["CardBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                Application.Current.Resources["BorderColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E1E5E9"));
                Application.Current.Resources["TextColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#212529"));
                Application.Current.Resources["TextSecondary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6C757D"));
                Application.Current.Resources["AccentColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4A6FA5"));
                Application.Current.Resources["AccentHover"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A5A95"));
                Application.Current.Resources["SuccessColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28A745"));
            }
        }

        private void ThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            ApplyTheme(ThemeToggle.IsChecked == true);
        }

        private void RefreshFormTheme()
        {
            // Обновляем цвета, которые не подхватываются автоматически
            var bgColor = (SolidColorBrush)Application.Current.Resources["BackgroundColor"];
            var cardColor = (SolidColorBrush)Application.Current.Resources["CardBackground"];
            var textColor = (SolidColorBrush)Application.Current.Resources["TextColor"];
            var textSecondary = (SolidColorBrush)Application.Current.Resources["TextSecondary"];
            var accentColor = (SolidColorBrush)Application.Current.Resources["AccentColor"];

            // Принудительно обновляем основные элементы
            this.Background = bgColor;

            // Обновляем прогресс-бар
            ProgressBar.Foreground = accentColor;

            // Обновляем статус
            UpdateCounters();
        }

        private void InitializeThemeResources()
        {
            // Создаем ресурсы если их нет
            if (!Application.Current.Resources.Contains("BackgroundColor"))
            {
                ApplyTheme(true); // Стартуем с темной темы
            }
        }

        private void ThemeToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            ApplyTheme(false); // Включаем светлую тему
        }

        // === ОСТАВШИЙСЯ КОД ГЕНЕРАЦИИ (без изменений) ===

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            // ... существующий код генерации ...
            if (DatePickerGen.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату генерации!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime date = DatePickerGen.SelectedDate.Value;
            var selectedTeachers = TeacherList.SelectedItems.Cast<DataRowView>().ToList();

            if (selectedTeachers.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы одного учителя!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Показать прогресс-бар
            ProgressBar.Visibility = Visibility.Visible;
            BtnGenerate.IsEnabled = false;

            try
            {
                // Симуляция длительной операции
                Task.Run(() =>
                {
                    // Твоя старая логика генерации тут
                    GenerateSchedule(date, selectedTeachers);
                }).ContinueWith(t =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ProgressBar.Visibility = Visibility.Collapsed;
                        BtnGenerate.IsEnabled = true;

                        if (t.IsFaulted && t.Exception != null)
                        {
                            MessageBox.Show($"Ошибка: {t.Exception.InnerException?.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                ProgressBar.Visibility = Visibility.Collapsed;
                BtnGenerate.IsEnabled = true;
            }
        }

        private void GenerateSchedule(DateTime date, List<DataRowView> selectedTeachers)
        {
            // ... существующий код генерации ...
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Загружаем слоты вручную (старый стиль)
                List<int> slots = new List<int>();
                SqlCommand getSlots = new SqlCommand("SELECT TimeSlotID FROM TimeSlot ORDER BY TimeStart", conn);
                SqlDataReader rdr = getSlots.ExecuteReader();
                while (rdr.Read())
                    slots.Add(rdr.GetInt32(0));
                rdr.Close();

                // Кэш
                Dictionary<int, int> classNumberCache = new Dictionary<int, int>();
                Dictionary<int, int> targetLessonsCache = new Dictionary<int, int>();
                Dictionary<int, int> lessonsCountCache = new Dictionary<int, int>();

                Random rnd = new Random();

                Func<int, int> getClassNumber = classId =>
                {
                    if (classNumberCache.ContainsKey(classId))
                        return classNumberCache[classId];

                    SqlCommand cmd = new SqlCommand("SELECT ClassName FROM SchoolClass WHERE ClassID=@cid", conn);
                    cmd.Parameters.AddWithValue("@cid", classId);
                    string cls = Convert.ToString(cmd.ExecuteScalar());
                    string digits = new string(cls.TakeWhile(char.IsDigit).ToArray());
                    int num = int.Parse(digits);

                    classNumberCache[classId] = num;
                    return num;
                };

                Func<int, int> getTargetLessons = classId =>
                {
                    if (targetLessonsCache.ContainsKey(classId))
                        return targetLessonsCache[classId];

                    int num = getClassNumber(classId);
                    int min, max;

                    if (num <= 4) { min = 3; max = 4; }
                    else if (num <= 8) { min = 4; max = 6; }
                    else { min = 5; max = 7; }

                    int t = rnd.Next(min, max + 1);
                    targetLessonsCache[classId] = t;
                    return t;
                };

                Func<int, int> getLessonsCount = classId =>
                {
                    if (lessonsCountCache.ContainsKey(classId))
                        return lessonsCountCache[classId];

                    SqlCommand cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM Schedule WHERE ClassID=@cid AND CAST(Date AS date)=@d",
                        conn);
                    cmd.Parameters.AddWithValue("@cid", classId);
                    cmd.Parameters.AddWithValue("@d", date);

                    int cnt = Convert.ToInt32(cmd.ExecuteScalar());
                    lessonsCountCache[classId] = cnt;
                    return cnt;
                };

                Action<int> incrementLesson = classId =>
                {
                    lessonsCountCache[classId] = getLessonsCount(classId) + 1;
                };

                foreach (DataRowView teacher in selectedTeachers)
                {
                    int teacherId = Convert.ToInt32(teacher["TeacherID"]);

                    string q = @"
                        SELECT ep.ProgramID, ep.ClassID, ep.DisciplineID,
                               ep.Quarter1Hours, ep.Quarter2Hours, ep.Quarter3Hours, ep.Quarter4Hours,
                               ISNULL(COUNT(s.ScheduleID), 0) AS UsedHours
                        FROM EducationalProgram ep
                        LEFT JOIN Schedule s 
                            ON s.ClassID = ep.ClassID 
                           AND s.DisciplineID = ep.DisciplineID
                           AND DATEPART(QUARTER, s.Date) = DATEPART(QUARTER, @Date)
                        WHERE ep.DisciplineID IN (SELECT DisciplineID FROM TeachingAssignment WHERE TeacherID=@tid)
                        GROUP BY ep.ProgramID, ep.ClassID, ep.DisciplineID, 
                                 ep.Quarter1Hours, ep.Quarter2Hours, ep.Quarter3Hours, ep.Quarter4Hours;";

                    SqlCommand cmd = new SqlCommand(q, conn);
                    cmd.Parameters.AddWithValue("@tid", teacherId);
                    cmd.Parameters.AddWithValue("@Date", date);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable programs = new DataTable();
                    da.Fill(programs);

                    foreach (DataRow row in programs.Rows)
                    {
                        int classId = Convert.ToInt32(row["ClassID"]);
                        int disciplineId = Convert.ToInt32(row["DisciplineID"]);

                        int target = getTargetLessons(classId);
                        int current = getLessonsCount(classId);

                        if (current >= target)
                            continue;

                        int used = Convert.ToInt32(row["UsedHours"]);
                        int allowed = Convert.ToInt32(row["Quarter" + (((date.Month - 1) / 3) + 1) + "Hours"]);

                        if (used >= allowed)
                            continue;

                        foreach (int slot in slots)
                        {
                            SqlCommand checkClass = new SqlCommand(
                                "SELECT COUNT(*) FROM Schedule WHERE ClassID=@cid AND CAST(Date AS date)=@d AND TimeSlotID=@s",
                                conn);
                            checkClass.Parameters.AddWithValue("@cid", classId);
                            checkClass.Parameters.AddWithValue("@d", date);
                            checkClass.Parameters.AddWithValue("@s", slot);

                            if (Convert.ToInt32(checkClass.ExecuteScalar()) > 0)
                                continue;

                            SqlCommand checkSubject = new SqlCommand(
                                "SELECT COUNT(*) FROM Schedule WHERE DisciplineID=@did AND CAST(Date AS date)=@d AND TimeSlotID=@s",
                                conn);
                            checkSubject.Parameters.AddWithValue("@did", disciplineId);
                            checkSubject.Parameters.AddWithValue("@d", date);
                            checkSubject.Parameters.AddWithValue("@s", slot);

                            if (Convert.ToInt32(checkSubject.ExecuteScalar()) > 0)
                                continue;

                            SqlCommand ins = new SqlCommand(
                                "INSERT INTO Schedule (Date, ClassID, DisciplineID, TimeSlotID) VALUES (@d, @cid, @did, @s)",
                                conn);
                            ins.Parameters.AddWithValue("@d", date);
                            ins.Parameters.AddWithValue("@cid", classId);
                            ins.Parameters.AddWithValue("@did", disciplineId);
                            ins.Parameters.AddWithValue("@s", slot);
                            ins.ExecuteNonQuery();

                            incrementLesson(classId);
                            break;
                        }
                    }
                }

                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Расписание сгенерировано БЕЗ конфликтов!", "Готово",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                });
            }
        }

        private int GetQuarter(DateTime date)
        {
            return (date.Month - 1) / 3 + 1;
        }

        private void FormGenerateSchedule_Loaded(object sender, RoutedEventArgs e)
        {
            // Применяем тему после загрузки
            ApplyTheme(true);
        }

    }
}