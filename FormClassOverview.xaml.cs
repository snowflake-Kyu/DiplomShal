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
    /// <summary>
    /// Логика взаимодействия для FormClassOverview.xaml
    /// </summary>
    /// 

    public class ClassBlock
    {
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public List<string> Lessons { get; set; }
        public int LessonCount => Lessons?.Count ?? 0;
    }

    public partial class FormClassOverview : Window
    {

        private string connectionString =
            "Data Source=kpkserver.kpk.local;Initial Catalog=KP_2024_Shalamov;Persist Security Info=True;User ID=user;Password=1234567";

        // здесь будем хранить полное расписание на день
        private DataTable _scheduleTable;

        private int _userRole;

        public FormClassOverview()
        {
            InitializeComponent();
        }
        public FormClassOverview(int userRole)
        {
            try
            {
                InitializeComponent();
                _userRole = userRole;

                ConfigureButtonsByRole();

                // Проверка, что DatePicker не null
                if (DatePickerOverview == null)
                    throw new Exception("DatePickerOverview is null после InitializeComponent");

                DatePickerOverview.SelectedDate = DateTime.Today;
                LoadOverview(DateTime.Today);

                // Если дошли сюда - показываем сообщение
                System.Diagnostics.Debug.WriteLine("Конструктор FormClassOverview выполнен успешно");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в конструкторе FormClassOverview:\n\n{ex.Message}\n\n{ex.StackTrace}",
                                "Критическая ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);

                // Важно: закрываем окно, чтобы оно не пыталось показаться
                Close();
                throw; // Пробрасываем исключение дальше
            }
        }

        private void ConfigureButtonsByRole()
        {
            if (_userRole == 1) // Ученик
            {
                // Скрываем кнопки управления расписанием
                BtnAddLesson.Visibility = Visibility.Collapsed;
                BtnEditLesson.Visibility = Visibility.Collapsed;
                BtnDeleteLesson.Visibility = Visibility.Collapsed;

                // Дополнительно: можно изменить заголовок или добавить информационное сообщение
                // Например, добавим текстовую подсказку
                var infoText = new TextBlock
                {
                    Text = "ℹ️ Для учеников доступ только для просмотра",
                    FontSize = 11,
                    Foreground = (SolidColorBrush)FindResource("TextSecondary"),
                    FontStyle = FontStyles.Italic,
                    Margin = new Thickness(0, 8, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                // Находим панель с кнопками и добавляем подсказку
                var buttonPanel = BtnAddLesson.Parent as StackPanel;
                if (buttonPanel != null)
                {
                    buttonPanel.Children.Add(infoText);
                }
            }
            else if (_userRole == 2) // Учитель
            {
                // Для учителя можно оставить все кнопки, но добавить ограничения
                // Например, учитель может редактировать только свои уроки
                // Это можно реализовать позже

                // Можно добавить подсказку для учителя
                var infoText = new TextBlock
                {
                    Text = "✏️ Учитель может редактировать расписание",
                    FontSize = 11,
                    Foreground = (SolidColorBrush)FindResource("SuccessColor"),
                    FontStyle = FontStyles.Italic,
                    Margin = new Thickness(0, 8, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                var buttonPanel = BtnAddLesson.Parent as StackPanel;
                if (buttonPanel != null)
                {
                    buttonPanel.Children.Add(infoText);
                }
            }
            else if (_userRole == 3) // Администратор
            {
                // Для администратора все кнопки видны
                // Можно добавить подсказку
                var infoText = new TextBlock
                {
                    Text = "🔧 Администратор: полный доступ",
                    FontSize = 11,
                    Foreground = (SolidColorBrush)FindResource("AccentColor"),
                    FontStyle = FontStyles.Italic,
                    Margin = new Thickness(0, 8, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                var buttonPanel = BtnAddLesson.Parent as StackPanel;
                if (buttonPanel != null)
                {
                    buttonPanel.Children.Add(infoText);
                }
            }
        }

        private void BtnReload_Click(object sender, RoutedEventArgs e)
        {
            if (DatePickerOverview.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            LoadOverview(DatePickerOverview.SelectedDate.Value);
        }

        /// <summary>
        /// Загружаем расписание на указанную дату
        /// и формируем блоки по классам (слева) + кэш для подробного отображения (справа).
        /// </summary>
        private void LoadOverview(DateTime date)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT 
                        s.ScheduleID,
                        s.TimeSlotID,
                        s.DisciplineID,
                        FORMAT(s.Date, 'dd.MM.yyyy') AS Date,
                        CONCAT(
                            CONVERT(varchar(5), CAST(ts.TimeStart AS time), 108),
                            ' - ',
                            CONVERT(varchar(5), CAST(ts.TimeEnd AS time), 108)
                        ) AS Time,
                        s.ClassID,
                        c.ClassName, 
                        d.Name AS Discipline,
                        ISNULL(t.FullName, N'—') AS Teacher,
                        ISNULL(a.Name, N'—') AS Auditorium
                    FROM Schedule s
                    JOIN TimeSlot ts ON s.TimeSlotID = ts.TimeSlotID
                    JOIN SchoolClass c ON s.ClassID = c.ClassID
                    JOIN Discipline d ON s.DisciplineID = d.DisciplineID
                    OUTER APPLY (
                        SELECT TOP 1 ta.TeacherID, ta.AuditoriumID
                        FROM TeachingAssignment ta
                        WHERE ta.DisciplineID = s.DisciplineID
                    ) AS ta
                    LEFT JOIN Teacher t ON ta.TeacherID = t.TeacherID
                    LEFT JOIN Auditorium a ON ta.AuditoriumID = a.AuditoriumID
                    WHERE CAST(s.Date AS date) = @Date
                    ORDER BY 
                        TRY_CAST(LEFT(c.ClassName, PATINDEX('%[^0-9]%', c.ClassName + 'X') - 1) AS INT),
                        c.ClassName,
                        ts.TimeStart;";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Date", date);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                _scheduleTable = dt; // сохранили для правой таблицы

                // --- формируем блоки по классам ---
                List<ClassBlock> blocks = dt
                    .AsEnumerable()
                    .GroupBy(r => new
                    {
                        ClassID = r.Field<int>("ClassID"),
                        ClassName = r.Field<string>("ClassName")
                    })
                    .Select(g => new ClassBlock
                    {
                        ClassID = g.Key.ClassID,
                        ClassName = g.Key.ClassName,
                        Lessons = g.Select(r => r.Field<string>("Discipline"))
                                   .Distinct()
                                   .ToList()
                    })
                    .OrderBy(b => b.ClassName)
                    .ToList();

                ClassBlocksList.ItemsSource = blocks;

                // очистить детальное отображение до выбора
                DetailsGrid.ItemsSource = null;
            }
        }

        /// <summary>
        /// При выборе блока класса слева – показываем детальное расписание справа
        /// </summary>
        private void ClassBlocksList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClassBlock selected = ClassBlocksList.SelectedItem as ClassBlock;
            if (selected == null || _scheduleTable == null)
            {
                DetailsGrid.ItemsSource = null;
                DetailsHeader.Text = "Выберите класс слева";
                return;
            }

            // фильтрация по ClassID
            DataView view = new DataView(_scheduleTable);
            view.RowFilter = "ClassID = " + selected.ClassID;

            DetailsGrid.ItemsSource = view;

            // заголовок: "Класс 10А · 23.11.2025"
            string dateText = "";
            if (DatePickerOverview.SelectedDate.HasValue)
                dateText = DatePickerOverview.SelectedDate.Value.ToString("dd.MM.yyyy");
            else if (view.Count > 0)
                dateText = Convert.ToString(view[0]["Date"]);

            DetailsHeader.Text = string.Format("Класс {0} · {1}", selected.ClassName, dateText);
        }

        // ===== Кнопки работы с расписанием =====
        private void BtnAddLesson_Click(object sender, RoutedEventArgs e)
        {

            FormGenerateSchedule wnd = new FormGenerateSchedule();
            wnd.Owner = this;
            wnd.Show();

        }

        private void BtnEditLesson_Click(object sender, RoutedEventArgs e)
        {
            // проверяем выбранный класс
            ClassBlock selectedBlock = ClassBlocksList.SelectedItem as ClassBlock;
            if (selectedBlock == null)
            {
                MessageBox.Show("Сначала выберите класс слева.", "Изменение урока",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DatePickerOverview.SelectedDate.HasValue)
            {
                MessageBox.Show("Не выбрана дата.", "Изменение урока",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime date = DatePickerOverview.SelectedDate.Value;
            int classId = selectedBlock.ClassID;

            // проверяем выбранную строку в таблице
            DataRowView row = DetailsGrid.SelectedItem as DataRowView;
            if (row == null)
            {
                MessageBox.Show("Выберите строку расписания для изменения.", "Изменение урока",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int scheduleId;
            int currentTimeSlotId;
            int currentDisciplineId;

            if (!int.TryParse(row["ScheduleID"].ToString(), out scheduleId))
            {
                MessageBox.Show("Не удалось определить идентификатор записи расписания.", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(row["TimeSlotID"].ToString(), out currentTimeSlotId))
            {
                MessageBox.Show("Не удалось получить TimeSlotID.", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(row["DisciplineID"].ToString(), out currentDisciplineId))
            {
                MessageBox.Show("Не удалось получить DisciplineID.", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // открываем окно редактирования
            EditLessonWindow dlg = new EditLessonWindow(connectionString,
                                                        classId,
                                                        currentTimeSlotId,
                                                        currentDisciplineId);
            dlg.Owner = this;
            bool? result = dlg.ShowDialog();
            if (result != true)
                return; // отменено

            int newSlotId = dlg.SelectedTimeSlotId;
            int newDiscId = dlg.SelectedDisciplineId;

            // если ничего не поменяли — выходим
            if (newSlotId == currentTimeSlotId && newDiscId == currentDisciplineId)
                return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // 1) проверка: в этом слоте у этого класса уже есть урок?
                SqlCommand checkClass = new SqlCommand(@"
        SELECT COUNT(*) 
        FROM Schedule 
        WHERE ClassID = @cid 
          AND CAST([Date] AS date) = @d
          AND TimeSlotID = @slot
          AND ScheduleID <> @id", conn);
                checkClass.Parameters.AddWithValue("@cid", classId);
                checkClass.Parameters.AddWithValue("@d", date);
                checkClass.Parameters.AddWithValue("@slot", newSlotId);
                checkClass.Parameters.AddWithValue("@id", scheduleId);

                int classConflict = Convert.ToInt32(checkClass.ExecuteScalar());
                if (classConflict > 0)
                {
                    MessageBox.Show("В этом временном слоте у класса уже есть другой урок.",
                                    "Конфликт расписания", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 2) БОЛЬШЕ НИКАКИХ ПРОВЕРОК НА ДРУГИЕ КЛАССЫ / УЧИТЕЛЕЙ

                // 3) обновляем запись
                SqlCommand update = new SqlCommand(@"
        UPDATE Schedule
        SET TimeSlotID = @slot, DisciplineID = @did
        WHERE ScheduleID = @id", conn);
                update.Parameters.AddWithValue("@slot", newSlotId);
                update.Parameters.AddWithValue("@did", newDiscId);
                update.Parameters.AddWithValue("@id", scheduleId);

                update.ExecuteNonQuery();
            }


            // перезагружаем расписание на ту же дату
            LoadOverview(date);
        }


        private void BtnDeleteLesson_Click(object sender, RoutedEventArgs e)
        {
            ClassBlock selectedBlock = ClassBlocksList.SelectedItem as ClassBlock;
            if (selectedBlock == null)
            {
                MessageBox.Show("Сначала выберите класс слева.", "Удаление",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DatePickerOverview.SelectedDate.HasValue)
            {
                MessageBox.Show("Не выбрана дата. Укажите дату сверху.", "Удаление",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime date = DatePickerOverview.SelectedDate.Value;
            int classId = selectedBlock.ClassID;

            // Диалог выбора варианта удаления
            DeleteScheduleWindow dlg = new DeleteScheduleWindow();
            dlg.Owner = this;
            bool? result = dlg.ShowDialog();
            if (result != true)
                return; // пользователь нажал Отмена

            string warnText;

            if (dlg.DeleteAllForDay)
            {
                warnText = string.Format(
                    "Вы уверены, что хотите удалить ВСЁ расписание на {0} по всем классам?",
                    date.ToString("dd.MM.yyyy"));
            }
            else
            {
                warnText = string.Format(
                    "Вы уверены, что хотите удалить все уроки класса {0} на {1}?",
                    selectedBlock.ClassName,
                    date.ToString("dd.MM.yyyy"));
            }

            if (MessageBox.Show(warnText, "Подтверждение удаления",
                                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            // Удаляем из базы
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                if (dlg.DeleteAllForDay)
                {
                    // Удаляем все уроки по всей школе на выбранную дату
                    SqlCommand cmd = new SqlCommand(
                        "DELETE FROM Schedule WHERE CAST([Date] AS date) = @d", conn);
                    cmd.Parameters.AddWithValue("@d", date);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    // Удаляем все уроки выбранного класса на выбранную дату
                    SqlCommand cmd = new SqlCommand(
                        "DELETE FROM Schedule WHERE ClassID = @cid AND CAST([Date] AS date) = @d", conn);
                    cmd.Parameters.AddWithValue("@cid", classId);
                    cmd.Parameters.AddWithValue("@d", date);
                    cmd.ExecuteNonQuery();
                }
            }

            // Перезагружаем расписание на эту же дату
            LoadOverview(date);
        }

        private void BtnMoreLessons_Click(object sender, RoutedEventArgs e)
        {
            FormDisciplines wnd = new FormDisciplines();
            wnd.Owner = this;
            wnd.ShowDialog();
        }

        private void BtnMoreTeachers_Click(object sender, RoutedEventArgs e)
        {
            FormTeachers wnd = new FormTeachers();
            wnd.Owner = this;
            wnd.ShowDialog();
        }

        private void BtnMoreGroups_Click(object sender, RoutedEventArgs e)
        {
            FormGroups wnd = new FormGroups();
            wnd.Owner = this;
            wnd.ShowDialog();
        }

        private void BtnMoreAuditoriums_Click(object sender, RoutedEventArgs e)
        {
            FormAuditoriums wnd = new FormAuditoriums();
            wnd.Owner = this;
            wnd.ShowDialog();
        }

        private void BtnMoreTeachingAssigment_Click(object sender, RoutedEventArgs e)
        {

            FormTeachingAssigment wnd = new FormTeachingAssigment();
            wnd.Owner = this;
            wnd.ShowDialog();

        }

        private void BtnMoreProgram_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Окно учебной программы");
        private void BtnMoreClasses_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Окно классов");

    }
}
