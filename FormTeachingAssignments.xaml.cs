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
    /// Логика взаимодействия для FormTeachingAssigment.xaml
    /// </summary>
    public partial class FormTeachingAssigment : Window
    {
        private string connectionString =
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=KP_2024_Shalamov;Integrated Security=True;";

        private DataTable _assignmentsTable;
        private DataTable _teachersTable;
        private DataTable _disciplinesTable;
        private DataTable _auditoriumsTable;

        private int _currentAssignmentId = 0;

        public FormTeachingAssigment()
        {
            InitializeComponent();
            LoadLookups();
            LoadAssignments(null); // все назначения
        }

        private void LoadLookups()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Учителя
                using (SqlCommand cmd = new SqlCommand("SELECT TeacherID, FullName FROM Teacher ORDER BY FullName", conn))
                {
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    _teachersTable = new DataTable();
                    da.Fill(_teachersTable);
                }

                // Добавим в фильтр пункт "Все"
                DataTable tmpTeachers = _teachersTable.Clone();
                DataRow all = tmpTeachers.NewRow();
                all["TeacherID"] = DBNull.Value;
                all["FullName"] = "Все учителя";
                tmpTeachers.Rows.Add(all);
                foreach (DataRow r in _teachersTable.Rows) tmpTeachers.ImportRow(r);
                CbFilterTeacher.ItemsSource = tmpTeachers.DefaultView;
                CbFilterTeacher.DisplayMemberPath = "FullName";
                CbFilterTeacher.SelectedValuePath = "TeacherID";
                CbFilterTeacher.SelectedIndex = 0;

                // Для редактора учителя
                CbTeacher.ItemsSource = _teachersTable.DefaultView;
                CbTeacher.DisplayMemberPath = "FullName";
                CbTeacher.SelectedValuePath = "TeacherID";

                // Дисциплины
                using (SqlCommand cmd = new SqlCommand("SELECT DisciplineID, Name FROM Discipline ORDER BY Name", conn))
                {
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    _disciplinesTable = new DataTable();
                    da.Fill(_disciplinesTable);
                }
                CbDiscipline.ItemsSource = _disciplinesTable.DefaultView;
                CbDiscipline.DisplayMemberPath = "Name";
                CbDiscipline.SelectedValuePath = "DisciplineID";

                // Аудитории
                using (SqlCommand cmd = new SqlCommand("SELECT AuditoriumID, Name FROM Auditorium ORDER BY Name", conn))
                {
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    _auditoriumsTable = new DataTable();
                    da.Fill(_auditoriumsTable);
                }
                CbAuditorium.ItemsSource = _auditoriumsTable.DefaultView;
                CbAuditorium.DisplayMemberPath = "Name";
                CbAuditorium.SelectedValuePath = "AuditoriumID";
            }
        }

        /// <summary>
        /// Загружает назначения. Если teacherFilterId == null — все, иначе только для указанного учителя.
        /// </summary>
        private void LoadAssignments(int? teacherFilterId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string where = "";
                if (teacherFilterId.HasValue)
                    where = "WHERE ta.TeacherID = @tid";

                string sql = $@"
SELECT ta.AssignmentID,
       ta.TeacherID,
       ISNULL(t.FullName, N'') AS FullName,
       ta.DisciplineID,
       ISNULL(d.Name, N'') AS DisciplineName,
       ta.AuditoriumID,
       ISNULL(a.Name, N'') AS AuditoriumName,
       ta.Comment
FROM TeachingAssignment ta
LEFT JOIN Teacher t ON ta.TeacherID = t.TeacherID
LEFT JOIN Discipline d ON ta.DisciplineID = d.DisciplineID
LEFT JOIN Auditorium a ON ta.AuditoriumID = a.AuditoriumID
{where}
ORDER BY t.FullName, d.Name;";

                SqlCommand cmd = new SqlCommand(sql, conn);
                if (teacherFilterId.HasValue)
                    cmd.Parameters.AddWithValue("@tid", teacherFilterId.Value);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                _assignmentsTable = new DataTable();
                da.Fill(_assignmentsTable);

                // Привязываем данные к обоим контролам
                AssignmentsGrid.ItemsSource = _assignmentsTable.DefaultView;
                AssignmentsListView.ItemsSource = _assignmentsTable.DefaultView;

                // Обновляем счетчик записей
                UpdateAssignmentCount();
            }

            ClearEditorAssignment();
        }

        private void UpdateAssignmentCount()
        {
            if (_assignmentsTable != null)
            {
                int count = _assignmentsTable.Rows.Count;
                // Ищем TextBlock для счетчика в XAML
                // Если в XAML есть элемент с именем TxtAssignmentCount, используем его
                // Иначе создаем или пропускаем
                var countTextBlock = FindName("TxtAssignmentCount") as TextBlock;
                if (countTextBlock != null)
                {
                    countTextBlock.Text = $"({count} записей)";
                }
            }
        }

        private void ClearEditorAssignment()
        {
            _currentAssignmentId = 0;
            if (CbTeacher.Items.Count > 0) CbTeacher.SelectedIndex = 0;
            if (CbDiscipline.Items.Count > 0) CbDiscipline.SelectedIndex = 0;
            if (CbAuditorium.Items.Count > 0) CbAuditorium.SelectedIndex = 0;
            TxtComment.Text = "";
            AssignmentsGrid.SelectedIndex = -1;
            AssignmentsListView.SelectedIndex = -1;
        }

        private void AssignmentsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Синхронизируем выделение между таблицей и ListView
            if (sender == AssignmentsGrid && AssignmentsListView != null)
            {
                // Обновляем ListView
                var selectedItem = AssignmentsGrid.SelectedItem as DataRowView;
                if (selectedItem != null)
                {
                    // Находим соответствующий элемент в ListView
                    foreach (var item in AssignmentsListView.Items)
                    {
                        if (item is DataRowView rowView && rowView["AssignmentID"].Equals(selectedItem["AssignmentID"]))
                        {
                            AssignmentsListView.SelectedItem = item;
                            break;
                        }
                    }
                }
                else
                {
                    AssignmentsListView.SelectedIndex = -1;
                }
            }

            // Основная логика обработки выбора
            DataRowView row = AssignmentsGrid.SelectedItem as DataRowView;
            if (row == null)
            {
                ClearEditorAssignment();
                return;
            }

            _currentAssignmentId = Convert.ToInt32(row["AssignmentID"]);
            if (row["TeacherID"] != DBNull.Value)
                CbTeacher.SelectedValue = Convert.ToInt32(row["TeacherID"]);
            else
                CbTeacher.SelectedIndex = -1;

            if (row["DisciplineID"] != DBNull.Value)
                CbDiscipline.SelectedValue = Convert.ToInt32(row["DisciplineID"]);
            else
                CbDiscipline.SelectedIndex = -1;

            if (row["AuditoriumID"] != DBNull.Value)
                CbAuditorium.SelectedValue = Convert.ToInt32(row["AuditoriumID"]);
            else
                CbAuditorium.SelectedIndex = -1;

            TxtComment.Text = row["Comment"] == DBNull.Value ? "" : row["Comment"].ToString();
        }

        // Добавим обработчик для ListView
        private void AssignmentsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == AssignmentsListView && AssignmentsGrid != null)
            {
                // Синхронизируем выделение с таблицей
                var selectedItem = AssignmentsListView.SelectedItem as DataRowView;
                if (selectedItem != null)
                {
                    // Находим соответствующий элемент в таблице
                    foreach (var item in AssignmentsGrid.Items)
                    {
                        if (item is DataRowView rowView && rowView["AssignmentID"].Equals(selectedItem["AssignmentID"]))
                        {
                            AssignmentsGrid.SelectedItem = item;
                            break;
                        }
                    }
                }
                else
                {
                    AssignmentsGrid.SelectedIndex = -1;
                }
            }
        }

        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            ClearEditorAssignment();
            CbTeacher.Focus();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // проверка
            if (CbTeacher.SelectedValue == null)
            {
                MessageBox.Show("Выберите учителя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (CbDiscipline.SelectedValue == null)
            {
                MessageBox.Show("Выберите дисциплину.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int teacherId = Convert.ToInt32(CbTeacher.SelectedValue);
            int disciplineId = Convert.ToInt32(CbDiscipline.SelectedValue);
            object auditoriumVal = (CbAuditorium.SelectedValue == null) ? (object)DBNull.Value : (object)CbAuditorium.SelectedValue;
            object commentVal = string.IsNullOrWhiteSpace(TxtComment.Text) ? (object)DBNull.Value : (object)TxtComment.Text.Trim();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                if (_currentAssignmentId == 0)
                {
                    // INSERT
                    SqlCommand insert = new SqlCommand(@"
INSERT INTO TeachingAssignment (TeacherID, DisciplineID, AuditoriumID, Comment)
VALUES (@tid, @did, @aid, @cmt);
SELECT SCOPE_IDENTITY();", conn);

                    insert.Parameters.AddWithValue("@tid", teacherId);
                    insert.Parameters.AddWithValue("@did", disciplineId);
                    insert.Parameters.AddWithValue("@aid", auditoriumVal);
                    insert.Parameters.AddWithValue("@cmt", commentVal);

                    object newId = insert.ExecuteScalar();
                    _currentAssignmentId = Convert.ToInt32(newId);
                }
                else
                {
                    // UPDATE
                    SqlCommand update = new SqlCommand(@"
UPDATE TeachingAssignment
SET TeacherID = @tid,
    DisciplineID = @did,
    AuditoriumID = @aid,
    Comment = @cmt
WHERE AssignmentID = @id", conn);

                    update.Parameters.AddWithValue("@tid", teacherId);
                    update.Parameters.AddWithValue("@did", disciplineId);
                    update.Parameters.AddWithValue("@aid", auditoriumVal);
                    update.Parameters.AddWithValue("@cmt", commentVal);
                    update.Parameters.AddWithValue("@id", _currentAssignmentId);

                    update.ExecuteNonQuery();
                }
            }

            // Перезагрузим таблицу с сохранением фильтра по учителю, если он был
            int? filter = null;
            if (CbFilterTeacher.SelectedValue != null && CbFilterTeacher.SelectedValue != DBNull.Value)
                filter = Convert.ToInt32(CbFilterTeacher.SelectedValue);

            LoadAssignments(filter);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_currentAssignmentId == 0)
            {
                MessageBox.Show("Выберите назначение для удаления.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Удалить выбранное назначение?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand delete = new SqlCommand("DELETE FROM TeachingAssignment WHERE AssignmentID = @id", conn);
                delete.Parameters.AddWithValue("@id", _currentAssignmentId);
                try
                {
                    delete.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Ошибка при удалении:\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            int? filter = null;
            if (CbFilterTeacher.SelectedValue != null && CbFilterTeacher.SelectedValue != DBNull.Value)
                filter = Convert.ToInt32(CbFilterTeacher.SelectedValue);

            LoadAssignments(filter);
        }

        private void CbFilterTeacher_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int? filter = null;
            if (CbFilterTeacher.SelectedValue != null && CbFilterTeacher.SelectedValue != DBNull.Value)
                filter = Convert.ToInt32(CbFilterTeacher.SelectedValue);

            LoadAssignments(filter);
        }

        private void BtnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            CbFilterTeacher.SelectedIndex = 0;
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            int? filter = null;
            if (CbFilterTeacher.SelectedValue != null && CbFilterTeacher.SelectedValue != DBNull.Value)
                filter = Convert.ToInt32(CbFilterTeacher.SelectedValue);
            LoadLookups();
            LoadAssignments(filter);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TxtFilterAssignments_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyAssignmentsTextFilter();
        }

        private void ApplyAssignmentsTextFilter()
        {
            if (_assignmentsTable == null)
                return;

            string txt = TxtFilterAssignments.Text.Trim().Replace("'", "''");

            // если пусто — сбрасываем только текстовый фильтр, но при этом сохраняем фильтр по учителю (если он выбран)
            if (string.IsNullOrEmpty(txt))
            {
                _assignmentsTable.DefaultView.RowFilter = "";
                return;
            }

            // ищем по полям: FullName, DisciplineName, AuditoriumName, Comment
            string filter = string.Format(
                "FullName LIKE '%{0}%' OR DisciplineName LIKE '%{0}%' OR AuditoriumName LIKE '%{0}%' OR Comment LIKE '%{0}%'",
                txt);

            _assignmentsTable.DefaultView.RowFilter = filter;

            // Обновляем счетчик после применения фильтра
            UpdateAssignmentCount();
        }
    }
}