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
    /// Логика взаимодействия для FormTeachers.xaml
    /// </summary>
    public partial class FormTeachers : Window
    {
        private string connectionString =
            "Data Source=kpkserver.kpk.local;Initial Catalog=KP_2024_Shalamov;Persist Security Info=True;User ID=user;Password=1234567";

        //Data Source=kpkserver.kpk.local;Initial Catalog=KP_2024_Shalamov;Persist Security Info=True;User ID=user;Password=1234567

        private DataTable _teachersTable;
        private DataTable _disciplinesTable;
        private DataTable _auditoriumsTable;

        private int _currentTeacherId = 0;

        public FormTeachers()
        {
            InitializeComponent();
            LoadDisciplines();
            LoadAuditoriums();
            LoadTeachers(); // без фильтра — все учителя
        }

        private void LoadDisciplines()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT DisciplineID, Name FROM Discipline ORDER BY Name", conn);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                _disciplinesTable = new DataTable();
                da.Fill(_disciplinesTable);

                // добавляем пункт "Все предметы"
                DataRow allRow = _disciplinesTable.NewRow();
                allRow["DisciplineID"] = DBNull.Value;
                allRow["Name"] = "Все предметы";
                _disciplinesTable.Rows.InsertAt(allRow, 0);

                // фильтр (в редакторе предметов мы больше не трогаем)
                CbFilterDiscipline.ItemsSource = _disciplinesTable.DefaultView;
                CbFilterDiscipline.SelectedIndex = 0;
            }
        }

        private void LoadAuditoriums()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT AuditoriumID, Name FROM Auditorium ORDER BY Name", conn);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                _auditoriumsTable = new DataTable();
                da.Fill(_auditoriumsTable);

                // оставляем аудиторийную таблицу — может пригодится в других формах
                _auditoriumsTable = _auditoriumsTable;
            }
        }

        /// <summary>
        /// Загружает учителей. Если передан filterDisciplineId — подгружаем только
        /// тех учителей, у которых есть назначение для этого предмета.
        /// В результате добавляем (через OUTER APPLY) одну строку из TeachingAssignment
        /// для отображения DisciplineName/AuditoriumName (если есть).
        /// </summary>
        private void LoadTeachers(int? filterDisciplineId = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // если передан фильтр по предмету — используем WHERE EXISTS
                string whereClause = "";
                if (filterDisciplineId.HasValue)
                    whereClause = "WHERE EXISTS (SELECT 1 FROM TeachingAssignment ta WHERE ta.TeacherID = t.TeacherID AND ta.DisciplineID = @filterDid)";

                string query = $@"
SELECT
    t.TeacherID,
    t.FullName,
    t.Email,
    -- берем одну назначенную дисциплину/аудиторию через OUTER APPLY (TOP 1)
    ta.DisciplineID,
    td.Name AS DisciplineName,
    ta.AuditoriumID,
    a.Name AS AuditoriumName
FROM Teacher t
OUTER APPLY (
    SELECT TOP 1 ta1.DisciplineID, ta1.AuditoriumID
    FROM TeachingAssignment ta1
    WHERE ta1.TeacherID = t.TeacherID
    ORDER BY ta1.AssignmentID
) ta
LEFT JOIN Discipline td ON ta.DisciplineID = td.DisciplineID
LEFT JOIN Auditorium a ON ta.AuditoriumID = a.AuditoriumID
{whereClause}
ORDER BY t.FullName;";

                SqlCommand cmd = new SqlCommand(query, conn);
                if (filterDisciplineId.HasValue)
                    cmd.Parameters.AddWithValue("@filterDid", filterDisciplineId.Value);

                SqlDataAdapter da = new SqlDataAdapter(cmd);

                _teachersTable = new DataTable();
                da.Fill(_teachersTable);

                TeachersGrid.ItemsSource = _teachersTable.DefaultView;
            }

            ClearEditor();
            ApplyTeacherFilter_LocalNameOnly(); // применяем локальный фильтр по ФИО, если есть
        }

        private void ClearEditor()
        {
            _currentTeacherId = 0;
            TxtFullName.Text = "";
            TxtEmail.Text = "";
            // в редакторе предмет/аудиторию удалили — редактируется отдельно в TeachingAssignment
        }

        // локальная фильтрация по ФИО (RowFilter) — применяется поверх уже загруженного набора.
        private void ApplyTeacherFilter_LocalNameOnly()
        {
            if (_teachersTable == null) return;

            string nameText = TxtFilterTeacher.Text.Trim().Replace("'", "''");
            if (string.IsNullOrEmpty(nameText))
                _teachersTable.DefaultView.RowFilter = "";
            else
                _teachersTable.DefaultView.RowFilter = $"FullName LIKE '%{nameText}%'";
        }

        private void TxtFilterTeacher_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyTeacherFilter_LocalNameOnly();
        }

        // при выборе предмета: перегружаем учителей с фильтром на предмет
        private void CbFilterDiscipline_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbFilterDiscipline.SelectedValue == null || CbFilterDiscipline.SelectedValue == DBNull.Value)
                LoadTeachers(null);
            else
            {
                int disId;
                if (int.TryParse(CbFilterDiscipline.SelectedValue.ToString(), out disId))
                    LoadTeachers(disId);
                else
                    LoadTeachers(null);
            }
        }

        private void TeachersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView rowView = TeachersGrid.SelectedItem as DataRowView;
            if (rowView == null)
            {
                ClearEditor();
                return;
            }

            _currentTeacherId = Convert.ToInt32(rowView["TeacherID"]);
            TxtFullName.Text = rowView["FullName"].ToString();
            TxtEmail.Text = rowView["Email"].ToString();
            // Discipline/Auditorium отображаются только для просмотра (через OUTER APPLY), а не для редактирования здесь
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            ClearEditor();
            TxtFullName.Focus();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtFullName.Text))
            {
                MessageBox.Show("Введите ФИО учителя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                if (_currentTeacherId == 0)
                {
                    // INSERT только FullName и Email
                    SqlCommand insert = new SqlCommand(@"
                        INSERT INTO Teacher (FullName, Email)
                        VALUES (@name, @mail);
                        SELECT SCOPE_IDENTITY();", conn);

                    insert.Parameters.AddWithValue("@name", TxtFullName.Text.Trim());
                    insert.Parameters.AddWithValue("@mail", string.IsNullOrWhiteSpace(TxtEmail.Text) ? (object)DBNull.Value : TxtEmail.Text.Trim());

                    object newIdObj = insert.ExecuteScalar();
                    _currentTeacherId = Convert.ToInt32(newIdObj);
                }
                else
                {
                    SqlCommand update = new SqlCommand(@"
                        UPDATE Teacher
                        SET FullName = @name,
                            Email = @mail
                        WHERE TeacherID = @id", conn);

                    update.Parameters.AddWithValue("@name", TxtFullName.Text.Trim());
                    update.Parameters.AddWithValue("@mail", string.IsNullOrWhiteSpace(TxtEmail.Text) ? (object)DBNull.Value : TxtEmail.Text.Trim());
                    update.Parameters.AddWithValue("@id", _currentTeacherId);

                    update.ExecuteNonQuery();
                }
            }

            // После сохранения перезагрузим учителей, сохранив текущий фильтр по предмету (если есть)
            if (CbFilterDiscipline.SelectedValue == null || CbFilterDiscipline.SelectedValue == DBNull.Value)
                LoadTeachers(null);
            else
            {
                int disId;
                if (int.TryParse(CbFilterDiscipline.SelectedValue.ToString(), out disId))
                    LoadTeachers(disId);
                else
                    LoadTeachers(null);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTeacherId == 0)
            {
                MessageBox.Show("Выберите учителя для удаления.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Удалить выбранного учителя? Если он используется в назначениях, могут возникнуть ошибки в расписании.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand delete = new SqlCommand("DELETE FROM Teacher WHERE TeacherID = @id", conn);
                delete.Parameters.AddWithValue("@id", _currentTeacherId);

                try
                {
                    delete.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Не удалось удалить учителя. Возможно, он уже привязан к назначению.\n\n" + ex.Message,
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // перезагрузим список с сохранением фильтра
            if (CbFilterDiscipline.SelectedValue == null || CbFilterDiscipline.SelectedValue == DBNull.Value)
                LoadTeachers(null);
            else
            {
                int disId;
                if (int.TryParse(CbFilterDiscipline.SelectedValue.ToString(), out disId))
                    LoadTeachers(disId);
                else
                    LoadTeachers(null);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
