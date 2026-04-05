using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using LiveCharts;
using LiveCharts.Wpf;

namespace WPFPPShall
{
    public partial class FormGroups : Window
    {
        private string connectionString =
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=KP_2024_Shalamov;Integrated Security=True;";

        private DataTable _groupsTable;
        private int _currentGroupId = 0;

        public FormGroups()
        {
            InitializeComponent();
            LoadGroups();
            LoadPieChart();

            // Скрываем комбобокс с классами, так как его нет в таблице
            CbClass.Visibility = Visibility.Collapsed;
            TextBlockClass.Visibility = Visibility.Collapsed;
        }

        private void LoadGroups()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Исправлено: используем реальные имена полей
                string query = @"
                    SELECT 
                        GroupID,
                        GroupName,
                        GroupSize
                    FROM StudentGroup
                    ORDER BY GroupName;";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                _groupsTable = new DataTable();
                da.Fill(_groupsTable);

                GroupsGrid.ItemsSource = _groupsTable.DefaultView;
            }

            ClearEditor();
        }

        private void LoadPieChart()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Для круговой диаграммы: показываем распределение по размеру групп
                string query = @"
                    SELECT 
                        GroupName,
                        ISNULL(GroupSize, 1) AS GroupSize
                    FROM StudentGroup
                    ORDER BY GroupName";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                var series = new SeriesCollection();
                int totalGroups = dt.Rows.Count;

                foreach (DataRow row in dt.Rows)
                {
                    string groupName = row["GroupName"].ToString();
                    int size = Convert.ToInt32(row["GroupSize"]);

                    // Если размер не указан, ставим 1
                    if (size <= 0) size = 1;

                    series.Add(new PieSeries
                    {
                        Title = groupName,
                        Values = new ChartValues<int> { size },
                        DataLabels = true,
                        LabelPoint = point => $"{point.Y} чел.",
                        FontSize = 10
                    });
                }

                GroupsPieChart.Series = series;
                TbTotalGroups.Text = totalGroups.ToString();
            }
        }

        private void ClearEditor()
        {
            _currentGroupId = 0;
            TxtGroupName.Text = "";
            TxtGroupSize.Text = "";
        }

        private void GroupsGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var rowView = GroupsGrid.SelectedItem as DataRowView;
            if (rowView == null)
            {
                ClearEditor();
                return;
            }

            // Исправлено: используем GroupID (без AS)
            if (rowView.Row.Table.Columns.Contains("GroupID"))
                _currentGroupId = Convert.ToInt32(rowView["GroupID"]);

            TxtGroupName.Text = rowView["GroupName"].ToString();

            // Добавляем загрузку размера группы
            if (rowView.Row.Table.Columns.Contains("GroupSize") && rowView["GroupSize"] != DBNull.Value)
                TxtGroupSize.Text = rowView["GroupSize"].ToString();
            else
                TxtGroupSize.Text = "";
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

            // Получаем размер группы (если введён)
            int? groupSize = null;
            if (!string.IsNullOrWhiteSpace(TxtGroupSize.Text))
            {
                if (int.TryParse(TxtGroupSize.Text, out int size))
                    groupSize = size;
                else
                    MessageBox.Show("Размер группы должен быть числом.", "Предупреждение",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                if (_currentGroupId == 0)
                {
                    // INSERT с GroupSize
                    SqlCommand insert = new SqlCommand(@"
                        INSERT INTO StudentGroup (GroupName, GroupSize)
                        VALUES (@name, @size);
                        SELECT SCOPE_IDENTITY();", conn);

                    insert.Parameters.AddWithValue("@name", TxtGroupName.Text.Trim());
                    insert.Parameters.AddWithValue("@size", groupSize.HasValue ? (object)groupSize.Value : DBNull.Value);

                    object newIdObj = insert.ExecuteScalar();
                    _currentGroupId = Convert.ToInt32(newIdObj);
                }
                else
                {
                    // UPDATE
                    SqlCommand update = new SqlCommand(@"
                        UPDATE StudentGroup
                        SET GroupName = @name,
                            GroupSize = @size
                        WHERE GroupID = @id", conn);

                    update.Parameters.AddWithValue("@name", TxtGroupName.Text.Trim());
                    update.Parameters.AddWithValue("@size", groupSize.HasValue ? (object)groupSize.Value : DBNull.Value);
                    update.Parameters.AddWithValue("@id", _currentGroupId);

                    update.ExecuteNonQuery();
                }
            }

            LoadGroups();
            LoadPieChart();
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
                "Удалить выбранную группу?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand delete = new SqlCommand(
                    "DELETE FROM StudentGroup WHERE GroupID = @id", conn);
                delete.Parameters.AddWithValue("@id", _currentGroupId);

                try
                {
                    delete.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(
                        "Не удалось удалить группу.\n\n" + ex.Message,
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            LoadGroups();
            LoadPieChart();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadGroups();
            LoadPieChart();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}