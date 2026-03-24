using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WPFPPShall
{
    public partial class FormCharts : Window
    {
        private string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=KP_2024_Shalamov;Integrated Security=True;";
        private List<DisciplineHours> disciplines = new List<DisciplineHours>();

        public class DisciplineHours
        {
            public string Name { get; set; }
            public int Hours { get; set; }
            public int ProgramCount { get; set; }

            public override string ToString()
            {
                return $"{Name} - {Hours} ч. (в {ProgramCount} программах)";
            }
        }

        public FormCharts()
        {
            InitializeComponent();
            LoadFromDatabase(); // ТОЛЬКО БД, нихуя не тест!
        }

        private void LoadFromDatabase()
        {
            try
            {
                disciplines.Clear();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            d.Name,
                            ISNULL(SUM(ep.HoursCount), 0) as TotalHours,
                            COUNT(ep.ProgramID) as ProgramsCount
                        FROM Discipline d
                        LEFT JOIN EducationalProgram ep ON d.DisciplineID = ep.DisciplineID
                        GROUP BY d.DisciplineID, d.Name
                        HAVING ISNULL(SUM(ep.HoursCount), 0) > 0
                        ORDER BY TotalHours DESC";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        disciplines.Add(new DisciplineHours
                        {
                            Name = reader["Name"].ToString(),
                            Hours = Convert.ToInt32(reader["TotalHours"]),
                            ProgramCount = Convert.ToInt32(reader["ProgramsCount"])
                        });
                    }
                }

                if (disciplines.Count == 0)
                {
                    MessageBox.Show("В базе данных нет учебных программ с часами!\n\n" +
                                   "Добавьте часы в EducationalProgram через форму учебной программы.",
                                   "Нет данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Close();
                    return;
                }

                UpdateListBox();
                DrawChart();
                TxtStatus.Text = $"✅ Загружено {disciplines.Count} дисциплин. Всего часов: {disciplines.Sum(d => d.Hours)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка БД",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void DrawChart()
        {
            ChartCanvas.Children.Clear();

            if (disciplines.Count == 0) return;

            // Настройки графика - ВСЁ ПОФИКСИЛ
            int canvasWidth = 700;
            int canvasHeight = 220;  // Уменьшил высоту
            int startX = 70;
            int startY = 50;  // Опустил оси вниз

            ChartCanvas.Width = canvasWidth + 100;
            ChartCanvas.Height = canvasHeight + 100;

            // Оси
            Line xAxis = new Line
            {
                X1 = startX - 10,
                Y1 = startY + canvasHeight,
                X2 = startX + canvasWidth,
                Y2 = startY + canvasHeight,
                Stroke = Brushes.Black,
                StrokeThickness = 1.5
            };
            ChartCanvas.Children.Add(xAxis);

            Line yAxis = new Line
            {
                X1 = startX,
                Y1 = startY - 10,
                X2 = startX,
                Y2 = startY + canvasHeight + 10,
                Stroke = Brushes.Black,
                StrokeThickness = 1.5
            };
            ChartCanvas.Children.Add(yAxis);

            // Максимальное значение
            int maxHours = disciplines.Max(d => d.Hours);
            if (maxHours == 0) maxHours = 1;
            maxHours = ((maxHours + 99) / 100) * 100;

            // Ширина столбца
            double colWidth = Math.Min(50, (canvasWidth - 100) / disciplines.Count);
            double spacing = 25;

            // Рисуем столбцы
            for (int i = 0; i < disciplines.Count; i++)
            {
                double x = startX + spacing + i * (colWidth + spacing);
                double colHeight = ((double)disciplines[i].Hours / maxHours) * (canvasHeight - 50);

                // Столбец
                Rectangle rect = new Rectangle
                {
                    Width = colWidth,
                    Height = colHeight,
                    Fill = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                    Stroke = Brushes.White,
                    StrokeThickness = 1,
                    RadiusX = 3,
                    RadiusY = 3
                };

                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, startY + canvasHeight - colHeight - 5);  // СТАВИМ СВЕРХУ, а не снизу!
                ChartCanvas.Children.Add(rect);

                // Значение сверху столбца
                if (disciplines[i].Hours > 0)
                {
                    TextBlock valueText = new TextBlock
                    {
                        Text = disciplines[i].Hours.ToString(),
                        FontSize = 11,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.Black,
                        Background = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                        Padding = new Thickness(3)
                    };

                    Canvas.SetLeft(valueText, x + colWidth / 2 - 12);
                    Canvas.SetTop(valueText, startY + canvasHeight - colHeight - 25);  // ТОЖЕ СВЕРХУ
                    ChartCanvas.Children.Add(valueText);
                }

                // Название дисциплины
                string name = disciplines[i].Name;
                if (name.Length > 10) name = name.Substring(0, 8) + "..";

                TextBlock nameText = new TextBlock
                {
                    Text = name,
                    FontSize = 10,
                    FontWeight = FontWeights.Medium,
                    TextAlignment = TextAlignment.Center,
                    Width = colWidth + 20,
                    Foreground = Brushes.Black
                };

                Canvas.SetLeft(nameText, x - 10);
                Canvas.SetTop(nameText, startY + canvasHeight + 5);  // ПОДПИСЬ СНИЗУ
                ChartCanvas.Children.Add(nameText);
            }

            // Шкала Y
            for (int i = 0; i <= 5; i++)
            {
                int val = maxHours * i / 5;
                double yPos = startY + canvasHeight - (canvasHeight * i / 5) - 5;

                Line line = new Line
                {
                    X1 = startX - 5,
                    Y1 = yPos,
                    X2 = startX,
                    Y2 = yPos,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 0.5,
                    StrokeDashArray = new DoubleCollection { 2, 2 }
                };
                ChartCanvas.Children.Add(line);

                TextBlock valText = new TextBlock
                {
                    Text = val.ToString(),
                    FontSize = 9,
                    Foreground = Brushes.Gray
                };
                Canvas.SetLeft(valText, startX - 35);
                Canvas.SetTop(valText, yPos - 8);
                ChartCanvas.Children.Add(valText);
            }

            // Подпись оси Y
            TextBlock yTitle = new TextBlock
            {
                Text = "Часы",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.DarkSlateGray
            };
            Canvas.SetLeft(yTitle, startX - 45);
            Canvas.SetTop(yTitle, startY - 20);
            ChartCanvas.Children.Add(yTitle);

            // Подпись оси X
            TextBlock xTitle = new TextBlock
            {
                Text = "Дисциплины",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.DarkSlateGray
            };
            Canvas.SetLeft(xTitle, startX + canvasWidth / 2 - 30);
            Canvas.SetTop(xTitle, startY + canvasHeight + 25);
            ChartCanvas.Children.Add(xTitle);

            // Заголовок
            TextBlock chartTitle = new TextBlock
            {
                Text = $"Учебные часы по дисциплинам (всего: {disciplines.Sum(d => d.Hours)} ч.)",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkBlue
            };
            Canvas.SetLeft(chartTitle, startX + 50);
            Canvas.SetTop(chartTitle, 5);
            ChartCanvas.Children.Add(chartTitle);
        }

        private void UpdateListBox()
        {
            DataList.ItemsSource = null;
            DataList.ItemsSource = disciplines;
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadFromDatabase();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DataList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataList.SelectedItem is DisciplineHours selected)
            {
                TxtStatus.Text = $"Выбрано: {selected.Name} - {selected.Hours} ч. ({selected.ProgramCount} учебных программ)";
            }
        }
    }
}