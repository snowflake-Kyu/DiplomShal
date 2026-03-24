using System;
using System.Collections.Generic;
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
using Microsoft.Win32;

namespace WPFPPShall
{
    /// <summary>
    /// Логика взаимодействия для FormLogin.xaml
    /// </summary>
    public partial class FormLogin : Window
    {
        public int UserRole { get; private set; } // 1 - ученик, 2 - учитель, 3 - админ

        public FormLogin()
        {
            InitializeComponent();
            CmbRole.SelectedIndex = 0; // По умолчанию учитель
        }

        // Кнопка для школьников - без пароля
        private void BtnStudent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var FCO = new FormClassOverview(1);
                FCO.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в BtnStudent_Click: {ex.Message}");
            }
        }

        // Вход для учителя/админа - с проверкой пароля
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (CmbRole.SelectedItem == null)
            {
                MessageBox.Show("Выберите роль!", "Ошибка");
                return;
            }

            // Получаем выбранный пункт
            var selectedItem = CmbRole.SelectedItem as ComboBoxItem;
            string roleText = selectedItem.Content.ToString();
            string correctPassword = selectedItem.Tag.ToString();

            // Проверяем пароль
            if (TxtPassword.Password != correctPassword)
            {
                MessageBox.Show("Неверный пароль!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Определяем роль
            if (roleText.Contains("Учитель"))
                UserRole = 2;
            else if (roleText.Contains("Администратор"))
                UserRole = 3;

            DialogResult = true;
            Close();
        }
    }
}
