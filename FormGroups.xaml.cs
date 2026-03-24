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
    /// Логика взаимодействия для FormGroups.xaml
    /// </summary>
    public partial class FormGroups : Window
    {
        private string connectionString =
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=KP_2024_Shalamov;Integrated Security=True;";

        private DataTable _groupsTable;
        private DataTable _classesTable;

        private int _currentGroupId = 0;

        public FormGroups()
        {
            InitializeComponent();
            LoadClasses();
            LoadGroups();
        }

        private void LoadClasses()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT ClassID, ClassName FROM SchoolClass ORDER BY ClassName", conn);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                _classesTable = new DataTable();
                da.Fill(_classesTable);

                CbClass.ItemsSource = _classesTable.DefaultView;
            }
        }

        private void LoadGroups()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT 
                        sg.GroupID,
                        sg.GroupName
                    FROM StudentGroup sg
                    ORDER BY sg.GroupName;";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                _groupsTable = new DataTable();
                da.Fill(_groupsTable);

                GroupsGrid.ItemsSource = _groupsTable.DefaultView;
            }

            ClearEditor();
        }

        private void ClearEditor()
        {
            _currentGroupId = 0;
            TxtGroupName.Text = "";
            if (CbClass.Items.Count > 0)
                CbClass.SelectedIndex = 0;
        }

        private void GroupsGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var rowView = GroupsGrid.SelectedItem as System.Data.DataRowView;
            if (rowView == null)
            {
                ClearEditor();
                return;
            }

            _currentGroupId = Convert.ToInt32(rowView["StudentGroupID"]); //Ошибка
            TxtGroupName.Text = rowView["GroupName"].ToString();

            if (rowView["ClassID"] != DBNull.Value)
                CbClass.SelectedValue = Convert.ToInt32(rowView["ClassID"]);
            else if (CbClass.Items.Count > 0)
                CbClass.SelectedIndex = 0;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            ClearEditor();
            TxtGroupName.Focus();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtGroupName.Text))
            {
                MessageBox.Show("Введите название группы.", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CbClass.SelectedValue == null)
            {
                MessageBox.Show("Выберите класс.", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int classId = Convert.ToInt32(CbClass.SelectedValue);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                if (_currentGroupId == 0)
                {
                    // INSERT
                    SqlCommand insert = new SqlCommand(@"
                        INSERT INTO StudentGroup (GroupName, ClassID)
                        VALUES (@name, @cid);
                        SELECT SCOPE_IDENTITY();", conn);

                    insert.Parameters.AddWithValue("@name", TxtGroupName.Text.Trim());
                    insert.Parameters.AddWithValue("@cid", classId);

                    object newIdObj = insert.ExecuteScalar();
                    _currentGroupId = Convert.ToInt32(newIdObj);
                }
                else
                {
                    // UPDATE
                    SqlCommand update = new SqlCommand(@"
                        UPDATE StudentGroup
                        SET GroupName = @name,
                            ClassID = @cid
                        WHERE StudentGroupID = @id", conn);

                    update.Parameters.AddWithValue("@name", TxtGroupName.Text.Trim());
                    update.Parameters.AddWithValue("@cid", classId);
                    update.Parameters.AddWithValue("@id", _currentGroupId);

                    update.ExecuteNonQuery();
                }
            }

            LoadGroups();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_currentGroupId == 0)
            {
                MessageBox.Show("Выберите группу для удаления.", "Удаление",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                "Удалить выбранную группу? Если она используется в других таблицах, могут возникнуть ошибки.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand delete = new SqlCommand(
                    "DELETE FROM StudentGroup WHERE StudentGroupID = @id", conn);
                delete.Parameters.AddWithValue("@id", _currentGroupId);

                try
                {
                    delete.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(
                        "Не удалось удалить группу. Возможно, она используется в других данных.\n\n" + ex.Message,
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            LoadGroups();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
