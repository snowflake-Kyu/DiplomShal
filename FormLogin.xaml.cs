using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WPFPPShall
{
    public partial class FormLogin : Window
    {
        public int UserRole { get; private set; } // 1 - ученик, 2 - учитель, 3 - админ

        private string currentCaptcha = "";
        private Random random = new Random();
        private int failedCaptchaAttempts = 0;
        private bool isPasswordVisible = false; // Флаг видимости пароля

        public FormLogin()
        {
            InitializeComponent();
            CmbRole.SelectedIndex = 0; // По умолчанию учитель
            GenerateCaptcha();

            // Простая анимация появления окна
            Loaded += (s, e) =>
            {
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3));
                BeginAnimation(OpacityProperty, fadeIn);
            };
        }

        // Генерация капчи
        private void GenerateCaptcha()
        {
            // Генерируем случайный код (буквы + цифры)
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ0123456789";
            var stringBuilder = new StringBuilder();

            for (int i = 0; i < 6; i++)
            {
                stringBuilder.Append(chars[random.Next(chars.Length)]);
            }

            currentCaptcha = stringBuilder.ToString();

            // Добавляем визуальные искажения (разные размеры букв)
            TxtCaptchaCode.Text = currentCaptcha;

            // Добавляем случайное форматирование для усложнения распознавания
            TxtCaptchaCode.FontSize = 24 + random.Next(8);
            TxtCaptchaCode.Opacity = 0.7 + (random.NextDouble() * 0.3);

            // Очищаем поле ввода и сообщение об ошибке
            TxtCaptcha.Text = "";
            TxtCaptchaMessage.Visibility = Visibility.Collapsed;
        }

        // Обновить капчу
        private void BtnRefreshCaptcha_Click(object sender, RoutedEventArgs e)
        {
            GenerateCaptcha();

            // Анимация обновления
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.1));
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.1));

            fadeOut.Completed += (s, _) =>
            {
                TxtCaptchaCode.BeginAnimation(TextBlock.OpacityProperty, fadeIn);
            };

            TxtCaptchaCode.BeginAnimation(TextBlock.OpacityProperty, fadeOut);
        }

        // Проверка капчи
        private bool ValidateCaptcha()
        {
            string userInput = TxtCaptcha.Text.Trim().ToUpper();

            if (string.IsNullOrEmpty(userInput))
            {
                ShowCaptchaError("Введите код с картинки!");
                return false;
            }

            if (userInput != currentCaptcha)
            {
                failedCaptchaAttempts++;
                ShowCaptchaError($"Неверный код! Осталось попыток: {3 - failedCaptchaAttempts}");

                if (failedCaptchaAttempts >= 3)
                {
                    ShowCaptchaError("Слишком много неверных попыток! Капча обновлена.");
                    GenerateCaptcha();
                    failedCaptchaAttempts = 0;
                }

                TxtCaptcha.Focus();
                TxtCaptcha.SelectAll();
                return false;
            }

            // Успешная проверка
            failedCaptchaAttempts = 0;
            return true;
        }

        private void ShowCaptchaError(string message)
        {
            TxtCaptchaMessage.Text = message;
            TxtCaptchaMessage.Visibility = Visibility.Visible;

            // Анимация ошибки (тряска поля ввода)
            var animation = new DoubleAnimation();
            animation.From = -5;
            animation.To = 5;
            animation.Duration = TimeSpan.FromSeconds(0.05);
            animation.AutoReverse = true;
            animation.RepeatBehavior = new RepeatBehavior(3);

            var translateTransform = new TranslateTransform();
            TxtCaptcha.RenderTransform = translateTransform;
            translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        // Получить текущий пароль (работает и для PasswordBox, и для TextBox)
        private string GetPassword()
        {
            // Ищем элемент в панели
            var passwordPanel = PasswordPanel as StackPanel;
            if (passwordPanel != null)
            {
                foreach (var child in passwordPanel.Children)
                {
                    if (child is PasswordBox passwordBox && passwordBox.Visibility == Visibility.Visible)
                    {
                        return passwordBox.Password;
                    }
                    else if (child is TextBox textBox && textBox.Visibility == Visibility.Visible)
                    {
                        return textBox.Text;
                    }
                }
            }
            return "";
        }

        // Обработчик изменения роли
        private void CmbRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbRole.SelectedItem is ComboBoxItem selectedItem)
            {
                string roleText = selectedItem.Content.ToString();

                // Если выбран ученик - блокируем ввод пароля и капчу
                if (roleText.Contains("Ученик"))
                {
                    // Отключаем элементы
                    var passwordPanel = PasswordPanel as StackPanel;
                    if (passwordPanel != null)
                    {
                        foreach (var child in passwordPanel.Children)
                        {
                            if (child is Control control)
                                control.IsEnabled = false;
                        }
                    }
                    BtnLogin.IsEnabled = false;
                    BtnLogin.Opacity = 0.5;
                    CaptchaPanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    // Включаем элементы
                    var passwordPanel = PasswordPanel as StackPanel;
                    if (passwordPanel != null)
                    {
                        foreach (var child in passwordPanel.Children)
                        {
                            if (child is Control control)
                                control.IsEnabled = true;
                        }
                    }
                    BtnLogin.IsEnabled = true;
                    BtnLogin.Opacity = 1;
                    CaptchaPanel.Visibility = Visibility.Visible;

                    // Очищаем пароль
                    ClearPassword();
                    GenerateCaptcha(); // Обновляем капчу при смене роли
                }
            }
        }

        // Очистить поле пароля
        private void ClearPassword()
        {
            var passwordPanel = PasswordPanel as StackPanel;
            if (passwordPanel != null)
            {
                foreach (var child in passwordPanel.Children)
                {
                    if (child is PasswordBox passwordBox)
                    {
                        passwordBox.Clear();
                    }
                    else if (child is TextBox textBox)
                    {
                        textBox.Clear();
                    }
                }
            }
        }

        // Кнопка для школьников - без пароля и капчи
        private void BtnStudent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Простая анимация нажатия
                var button = sender as Button;
                if (button != null)
                {
                    var animation = new DoubleAnimation(0.95, TimeSpan.FromSeconds(0.05));
                    animation.Completed += (s, _) =>
                    {
                        var resetAnimation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.05));
                        button.BeginAnimation(Button.OpacityProperty, resetAnimation);
                    };
                    button.BeginAnimation(Button.OpacityProperty, animation);
                }

                // Открываем форму для ученика и закрываем текущую
                var FCO = new FormClassOverview(1);
                FCO.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе ученика: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Вход для учителя/админа - с проверкой пароля и капчи
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (CmbRole.SelectedItem == null)
            {
                ShowWarningMessage("Выберите роль пользователя!");
                return;
            }

            // Получаем выбранный пункт
            var selectedItem = CmbRole.SelectedItem as ComboBoxItem;
            string roleText = selectedItem.Content.ToString();
            string correctPassword = selectedItem.Tag?.ToString() ?? "";

            // Получаем пароль из поля (работает и для PasswordBox, и для TextBox)
            string enteredPassword = GetPassword();

            // Проверяем пароль
            if (enteredPassword != correctPassword)
            {
                ShowWarningMessage("Неверный пароль!\nПопробуйте еще раз.");
                ClearPassword();
                FocusPassword();
                return;
            }

            // Проверяем капчу
            if (!ValidateCaptcha())
            {
                return;
            }

            // Определяем роль
            if (roleText.Contains("Учитель"))
                UserRole = 2;
            else if (roleText.Contains("Администратор"))
                UserRole = 3;
            else
            {
                ShowWarningMessage("Некорректная роль!");
                return;
            }

            // Открываем соответствующую форму в зависимости от роли
            try
            {
                if (UserRole == 2) // Учитель
                {
                    var formClassOverview = new FormClassOverview(2);
                    formClassOverview.Show();
                }
                else if (UserRole == 3) // Администратор
                {
                    var formClassOverview = new FormClassOverview(3);
                    formClassOverview.Show();
                }

                // Закрываем окно авторизации
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Установить фокус на поле пароля
        private void FocusPassword()
        {
            var passwordPanel = PasswordPanel as StackPanel;
            if (passwordPanel != null)
            {
                foreach (var child in passwordPanel.Children)
                {
                    if (child is PasswordBox passwordBox)
                    {
                        passwordBox.Focus();
                        break;
                    }
                    else if (child is TextBox textBox)
                    {
                        textBox.Focus();
                        textBox.SelectAll();
                        break;
                    }
                }
            }
        }

        // Обработка нажатия Enter в поле капчи
        private void TxtCaptcha_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && BtnLogin.IsEnabled)
            {
                BtnLogin_Click(sender, e);
            }
        }

        // Показать пароль
        private void ShowPasswordCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (isPasswordVisible) return;

            var passwordPanel = PasswordPanel as StackPanel;
            if (passwordPanel != null)
            {
                // Ищем текущий PasswordBox
                PasswordBox currentPasswordBox = null;
                int index = -1;

                for (int i = 0; i < passwordPanel.Children.Count; i++)
                {
                    if (passwordPanel.Children[i] is PasswordBox pb)
                    {
                        currentPasswordBox = pb;
                        index = i;
                        break;
                    }
                }

                if (currentPasswordBox != null && index >= 0)
                {
                    // Создаем TextBox для отображения пароля
                    var tempTextBox = new TextBox
                    {
                        Text = currentPasswordBox.Password,
                        Height = 45,
                        FontSize = 14,
                        Background = Brushes.White,
                        BorderBrush = new SolidColorBrush(Color.FromRgb(225, 232, 237)),
                        BorderThickness = new Thickness(1),
                        Padding = new Thickness(12, 10, 12, 10),
                        VerticalContentAlignment = VerticalAlignment.Center
                    };

                    // Заменяем
                    passwordPanel.Children.RemoveAt(index);
                    passwordPanel.Children.Insert(index, tempTextBox);
                    isPasswordVisible = true;
                }
            }
        }

        private void ShowPasswordCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isPasswordVisible) return;

            var passwordPanel = PasswordPanel as StackPanel;
            if (passwordPanel != null)
            {
                // Ищем текущий TextBox
                TextBox currentTextBox = null;
                int index = -1;

                for (int i = 0; i < passwordPanel.Children.Count; i++)
                {
                    if (passwordPanel.Children[i] is TextBox tb)
                    {
                        currentTextBox = tb;
                        index = i;
                        break;
                    }
                }

                if (currentTextBox != null && index >= 0)
                {
                    // Создаем новый PasswordBox
                    var newPasswordBox = new PasswordBox
                    {
                        Height = 45,
                        FontSize = 14,
                        Background = Brushes.White,
                        BorderBrush = new SolidColorBrush(Color.FromRgb(225, 232, 237)),
                        BorderThickness = new Thickness(1),
                        Padding = new Thickness(12, 10, 12, 10),
                        Password = currentTextBox.Text
                    };

                    // Заменяем
                    passwordPanel.Children.RemoveAt(index);
                    passwordPanel.Children.Insert(index, newPasswordBox);
                    isPasswordVisible = false;
                }
            }
        }

        private void ShowWarningMessage(string message)
        {
            MessageBox.Show(message, "Внимание",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}