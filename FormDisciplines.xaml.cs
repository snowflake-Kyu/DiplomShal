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
    /// Логика взаимодействия для FormDisciplines.xaml
    /// </summary>
    public partial class FormDisciplines : Window
    {
        private string connectionString =
            "Data Source=kpkserver.kpk.local;Initial Catalog=KP_2024_Shalamov;Persist Security Info=True;User ID=user;Password=1234567";

        private DataTable _disciplinesTable;

        // ID текущей выбранной дисциплины (0 – новая)
        private int _currentDisciplineId = 0;

        private DateTime _lastVoteTime = DateTime.MinValue;
        private int _selectedDisciplineId = 0;

        public FormDisciplines()
        {
            InitializeComponent();
            LoadDisciplines();
            LoadRatings();
        }

        private void LoadDisciplines()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT d.DisciplineID, d.Name, d.ShortDescription
                    FROM Discipline d
                    ORDER BY d.Name;";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                _disciplinesTable = new DataTable();
                da.Fill(_disciplinesTable);

                DisciplinesGrid.ItemsSource = _disciplinesTable.DefaultView;
            }

            ClearEditor();
        }

        private void ClearEditor()
        {
            _currentDisciplineId = 0;
            TxtName.Text = "";
            TxtShortDescription.Text = "";
        }

        private void TxtFilterDiscipline_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_disciplinesTable == null)
                return;

            string text = TxtFilterDiscipline.Text.Trim().Replace("'", "''");

            if (string.IsNullOrEmpty(text))
            {
                _disciplinesTable.DefaultView.RowFilter = "";
            }
            else
            {
                _disciplinesTable.DefaultView.RowFilter = $"Name LIKE '%{text}%'";
            }
        }

        private void DisciplinesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var rowView = DisciplinesGrid.SelectedItem as DataRowView;
            if (rowView == null)
            {
                ClearEditor();
                TxtSelectedSubject.Text = "[выберите предмет]";
                _selectedDisciplineId = 0;
                return;
            }

            _selectedDisciplineId = Convert.ToInt32(rowView["DisciplineID"]);
            TxtSelectedSubject.Text = rowView["Name"].ToString();

            _currentDisciplineId = _selectedDisciplineId;
            TxtName.Text = rowView["Name"].ToString();
            TxtShortDescription.Text = rowView["ShortDescription"].ToString();
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
                MessageBox.Show("Введите название предмета.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // корректно готовим описание (NULL если пусто)
            object descr = string.IsNullOrWhiteSpace(TxtShortDescription.Text)
                ? (object)DBNull.Value
                : TxtShortDescription.Text.Trim();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                if (_currentDisciplineId == 0)
                {
                    // INSERT
                    SqlCommand insert = new SqlCommand(@"
                        INSERT INTO Discipline (Name, ShortDescription)
                        VALUES (@name, @descr);
                        SELECT SCOPE_IDENTITY();", conn);

                    insert.Parameters.AddWithValue("@name", TxtName.Text.Trim());
                    insert.Parameters.AddWithValue("@descr", descr);

                    var newIdObj = insert.ExecuteScalar();
                    _currentDisciplineId = Convert.ToInt32(newIdObj);
                }
                else
                {
                    // UPDATE
                    SqlCommand update = new SqlCommand(@"
                        UPDATE Discipline
                        SET Name = @name,
                            ShortDescription = @descr
                        WHERE DisciplineID = @id", conn);

                    update.Parameters.AddWithValue("@name", TxtName.Text.Trim());
                    update.Parameters.AddWithValue("@descr", descr);
                    update.Parameters.AddWithValue("@id", _currentDisciplineId);

                    update.ExecuteNonQuery();
                }
            }

            LoadDisciplines();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_currentDisciplineId == 0)
            {
                MessageBox.Show("Выберите предмет для удаления.", "Удаление",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                "Удалить выбранный предмет? Учебная программа и расписание, связанные с ним, могут перестать быть корректными.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand delete = new SqlCommand(
                    "DELETE FROM Discipline WHERE DisciplineID = @id", conn);
                delete.Parameters.AddWithValue("@id", _currentDisciplineId);

                try
                {
                    delete.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Не удалось удалить предмет. Возможно, он используется в учебной программе или расписании.\n\n" +
                                    ex.Message,
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            LoadDisciplines();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        

        private void BtnVoteGreen_Click(object sender, RoutedEventArgs e)
        {
            Vote(1); // RateID = 1 (Легко) = +1 балл
        }

        private void BtnVoteYellow_Click(object sender, RoutedEventArgs e)
        {
            Vote(2); // RateID = 2 (Нормально) = 0 баллов
        }

        private void BtnVoteRed_Click(object sender, RoutedEventArgs e)
        {
            Vote(3); // RateID = 3 (Сложно) = -1 балл
        }

        private void Vote(int rateId)
        {
            if (_selectedDisciplineId == 0)
            {
                MessageBox.Show("Сначала выбери предмет из списка слева!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка на 3 минуты
            if ((DateTime.Now - _lastVoteTime).TotalMinutes < 3)
            {
                TimeSpan remaining = TimeSpan.FromMinutes(3) - (DateTime.Now - _lastVoteTime);
                MessageBox.Show($"Подожди ещё {remaining.Minutes}:{remaining.Seconds:D2} минут(ы)!",
                    "Слишком часто", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Вставляем голос
                    SqlCommand cmd = new SqlCommand(@"
                INSERT INTO Vote (DisciplineID, RateID)
                VALUES (@discId, @rateId)", conn);

                    cmd.Parameters.AddWithValue("@discId", _selectedDisciplineId);
                    cmd.Parameters.AddWithValue("@rateId", rateId);
                    cmd.ExecuteNonQuery();
                }

                _lastVoteTime = DateTime.Now;
                UpdateTimerDisplay();
                LoadRatings(); // перегрузить рейтинги
                MessageBox.Show("Голос учтён! Спасибо", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Упс",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRatings()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"
                SELECT 
                    d.Name,
                    COUNT(v.VoteID) AS TotalVotes,
                    ISNULL(SUM(CASE WHEN v.RateID = 1 THEN 1 WHEN v.RateID = 3 THEN -1 ELSE 0 END), 0) AS Score
                FROM Discipline d
                LEFT JOIN Vote v ON d.DisciplineID = v.DisciplineID
                GROUP BY d.DisciplineID, d.Name
                HAVING COUNT(v.VoteID) > 0  -- только те, за которые голосовали
                ORDER BY Score DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Преобразуем в список RatingItem
                    var ratings = new List<RatingItem>();
                    foreach (DataRow row in dt.Rows)
                    {
                        ratings.Add(new RatingItem
                        {
                            Name = row["Name"].ToString(),
                            TotalVotes = Convert.ToInt32(row["TotalVotes"]),
                            Score = Convert.ToInt32(row["Score"])
                        });
                    }

                    // Привязываем к гриду
                    if (RatingsGrid != null)
                    {
                        RatingsGrid.ItemsSource = ratings;
                    }

                    // Напиши так:
                    if (RatingsGrid != null)  // если используешь DataGrid
                    {
                        RatingsGrid.ItemsSource = ratings;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки рейтингов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateTimerDisplay()
        {
            if ((DateTime.Now - _lastVoteTime).TotalMinutes < 3)
            {
                TimeSpan remaining = TimeSpan.FromMinutes(3) - (DateTime.Now - _lastVoteTime);
                TxtVoteTimer.Text = $"⏱️ Подожди {remaining.Minutes}:{remaining.Seconds:D2}";
            }
            else
            {
                TxtVoteTimer.Text = "⏱️ Можно голосовать";
            }
        }

        public class RatingItem
        {
            public string Name { get; set; }
            public int TotalVotes { get; set; }
            public int Score { get; set; }

            // Средний балл (от -1 до +1)
            public double AverageScore
            {
                get
                {
                    if (TotalVotes == 0) return 0;
                    return (double)Score / TotalVotes;
                }
            }

            // Отмасштабированный рейтинг (-100 до +100)
            public double ScaledScore
            {
                get
                {
                    return AverageScore * 100;
                }
            }

            // Для отображения в тексте
            public string DisplayScore
            {
                get
                {
                    if (TotalVotes == 0) return "0";
                    return ScaledScore.ToString("+#;-#;0");
                }
            }

            // Цвет текста в зависимости от рейтинга
            public SolidColorBrush ScoreColor
            {
                get
                {
                    if (ScaledScore > 30) return new SolidColorBrush(Colors.Green);
                    if (ScaledScore < -30) return new SolidColorBrush(Colors.Red);
                    return new SolidColorBrush(Colors.Orange);
                }
            }

            // Цвет прогресс-бара
            public SolidColorBrush BarColor
            {
                get
                {
                    if (ScaledScore > 30) return new SolidColorBrush(Color.FromRgb(16, 185, 129));  // зеленый
                    if (ScaledScore < -30) return new SolidColorBrush(Color.FromRgb(239, 68, 68));  // красный
                    return new SolidColorBrush(Color.FromRgb(245, 158, 11));  // желтый
                }
            }

            // Ширина бара (0-100%)
            public double BarWidth
            {
                get
                {
                    if (TotalVotes == 0) return 50; // нет голосов - по центру

                    // ScaledScore от -100 до +100 -> percent от 0 до 100
                    double percent = (ScaledScore + 100) * 100 / 200;

                    // Защита от погрешностей
                    if (percent < 0) percent = 0;
                    if (percent > 100) percent = 100;

                    return percent;
                }
            }
        }
    }
}