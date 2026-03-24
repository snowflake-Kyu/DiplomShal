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
    /// Логика взаимодействия для EditLessonWindow.xaml
    /// </summary>
    public partial class EditLessonWindow : Window
    {
        private readonly string _connectionString;
        private readonly int _classId;

        public int SelectedTimeSlotId { get; private set; }
        public int SelectedDisciplineId { get; private set; }

        public EditLessonWindow(string connectionString,
                                int classId,
                                int currentTimeSlotId,
                                int currentDisciplineId)
        {
            InitializeComponent();

            _connectionString = connectionString;
            _classId = classId;

            LoadTimeSlots(currentTimeSlotId);
            LoadDisciplines(currentDisciplineId);
        }

        private void LoadTimeSlots(int currentTimeSlotId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT TimeSlotID, CONCAT(CONVERT(varchar(5), TimeStart, 108), ' - ', CONVERT(varchar(5), TimeEnd, 108)) AS Title FROM TimeSlot ORDER BY TimeStart",
                    conn);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cbTimeSlot.ItemsSource = dt.DefaultView;
                cbTimeSlot.DisplayMemberPath = "Title";
                cbTimeSlot.SelectedValuePath = "TimeSlotID";
                cbTimeSlot.SelectedValue = currentTimeSlotId;
            }
        }

        private void LoadDisciplines(int currentDisciplineId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // берём предметы из учебной программы для выбранного класса
                SqlCommand cmd = new SqlCommand(@"
                    SELECT DISTINCT d.DisciplineID, d.Name
                    FROM EducationalProgram ep
                    JOIN Discipline d ON ep.DisciplineID = d.DisciplineID
                    WHERE ep.ClassID = @cid
                    ORDER BY d.Name;", conn);
                cmd.Parameters.AddWithValue("@cid", _classId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cbDiscipline.ItemsSource = dt.DefaultView;
                cbDiscipline.DisplayMemberPath = "Name";
                cbDiscipline.SelectedValuePath = "DisciplineID";
                cbDiscipline.SelectedValue = currentDisciplineId;
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (cbTimeSlot.SelectedValue == null || cbDiscipline.SelectedValue == null)
            {
                MessageBox.Show("Выберите время и предмет.", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedTimeSlotId = Convert.ToInt32(cbTimeSlot.SelectedValue);
            SelectedDisciplineId = Convert.ToInt32(cbDiscipline.SelectedValue);

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
