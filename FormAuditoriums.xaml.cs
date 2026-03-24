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
    /// Логика взаимодействия для FormAuditoriums.xaml
    /// </summary>
    public partial class FormAuditoriums : Window
    {
        private string connectionString =
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=KP_2024_Shalamov;Integrated Security=True;";

        private DataTable _auditoriumsTable;
        private int _currentAuditoriumId = 0;

        public FormAuditoriums()
        {
            InitializeComponent();
            LoadAuditoriums();
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

                AudGrid.ItemsSource = _auditoriumsTable.DefaultView;
            }

            ClearEditor();
        }

        private void ClearEditor()
        {
            _currentAuditoriumId = 0;
            TxtName.Text = "";
        }

        private void AudGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var rowView = AudGrid.SelectedItem as DataRowView;
            if (rowView == null)
            {
                ClearEditor();
                return;
            }

            _currentAuditoriumId = Convert.ToInt32(rowView["AuditoriumID"]);
            TxtName.Text = rowView["Name"].ToString();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            ClearEditor();
            TxtName.Focus();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                MessageBox.Show("Введите название аудитории.", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                if (_currentAuditoriumId == 0)
                {
                    // INSERT
                    SqlCommand insert = new SqlCommand(@"
                        INSERT INTO Auditorium (Name)
                        VALUES (@name);
                        SELECT SCOPE_IDENTITY();", conn);

                    insert.Parameters.AddWithValue("@name", TxtName.Text.Trim());

                    object newIdObj = insert.ExecuteScalar();
                    _currentAuditoriumId = Convert.ToInt32(newIdObj);
                }
                else
                {
                    // UPDATE
                    SqlCommand update = new SqlCommand(@"
                        UPDATE Auditorium
                        SET Name = @name
                        WHERE AuditoriumID = @id", conn);

                    update.Parameters.AddWithValue("@name", TxtName.Text.Trim());
                    update.Parameters.AddWithValue("@id", _currentAuditoriumId);

                    update.ExecuteNonQuery();
                }
            }

            LoadAuditoriums();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_currentAuditoriumId == 0)
            {
                MessageBox.Show("Выберите аудиторию для удаления.", "Удаление",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                "Удалить выбранную аудиторию? Если она используется в расписании или назначениях, могут возникнуть ошибки.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand delete = new SqlCommand(
                    "DELETE FROM Auditorium WHERE AuditoriumID = @id", conn);
                delete.Parameters.AddWithValue("@id", _currentAuditoriumId);

                try
                {
                    delete.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(
                        "Не удалось удалить аудиторию. Возможно, она уже используется в расписании или назначениях.\n\n" + ex.Message,
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            LoadAuditoriums();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
