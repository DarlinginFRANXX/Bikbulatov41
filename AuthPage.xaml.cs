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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Bikbulatov41
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        private string currentCaptcha;
        private DispatcherTimer blockTimer;
        private int blockTimeRemaining = 0;
        public AuthPage()
        {
            InitializeComponent();
            GenerateCaptcha();
            InitializeTimer();
        }
        private void InitializeTimer()
        {
            blockTimer = new DispatcherTimer();
            blockTimer.Interval = TimeSpan.FromSeconds(1);
            blockTimer.Tick += BlockTimer_Tick;
        }
        private void GenerateCaptcha()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            StringBuilder captchaBuilder = new StringBuilder();

            for (int i = 0; i < 6; i++)
            {
                captchaBuilder.Append(chars[random.Next(chars.Length)]);
            }

            currentCaptcha = captchaBuilder.ToString();
            CaptchaTextBlock.Text = currentCaptcha;
            CaptchaInputBox.Text = ""; // Очищаем поле ввода
            CaptchaInputBox.IsEnabled = true;
        }
        private bool ValidateCaptcha()
        {
            string userInput = CaptchaInputBox.Text.Trim();

            // Игнорируем регистр при проверке
            return string.Equals(userInput, currentCaptcha, StringComparison.OrdinalIgnoreCase);
        }

        // Блокировка кнопки входа на 10 секунд
        private void BlockLoginButton(int seconds)
        {
            LoginButton.IsEnabled = false;
            blockTimeRemaining = seconds;

            // Обновляем текст кнопки
            UpdateLoginButtonText();

            blockTimer.Start();
        }

        // Разблокировка кнопки
        private void UnblockLoginButton()
        {
            LoginButton.IsEnabled = true;
            LoginButton.Content = "Войти";
            blockTimer.Stop();
            blockTimeRemaining = 0;
        }
        private void UpdateLoginButtonText()
        {
            if (blockTimeRemaining > 0)
            {
                LoginButton.Content = $"Заблокировано ({blockTimeRemaining} сек)";
            }
        }

        // Таймер для отсчета блокировки
        private void BlockTimer_Tick(object sender, EventArgs e)
        {
            blockTimeRemaining--;

            if (blockTimeRemaining <= 0)
            {
                UnblockLoginButton();
            }
            else
            {
                UpdateLoginButtonText();
            }
        }

        // Кнопка "Обновить капчу"
        
        private void LoginHowGuestButton_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ProductPage(null));
            
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            if (blockTimer != null && blockTimer.IsEnabled)
            {
                blockTimer.Stop();
            }
        }


        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, не заблокирована ли кнопка
            if (!LoginButton.IsEnabled)
            {
                MessageBox.Show($"Попробуйте снова через {blockTimeRemaining} секунд", "Кнопка заблокирована");
                return;
            }

            string login = LoginBox.Text;
            string password = PasswordBox.Text;
            string captchaInput = CaptchaInputBox.Text;

            // Проверка на пустые поля
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Заполните логин и пароль", "Ошибка");
                return;
            }

            if (string.IsNullOrWhiteSpace(captchaInput))
            {
                MessageBox.Show("Введите капчу", "Ошибка");
                return;
            }

            // Проверка капчи
            if (!ValidateCaptcha())
            {
                MessageBox.Show("Неверная капча! Попробуйте снова.", "Ошибка");
                GenerateCaptcha(); // Генерируем новую капчу

                // Блокируем кнопку на 10 секунд
                BlockLoginButton(10);
                return;
            }

            // Поиск пользователя в базе данных
            User user = Bikbulatov41Entities.GetContext().User
                .FirstOrDefault(p => p.UserLogin == login && p.UserPassword == password);

            if (user != null)
            {
                Manager.MainFrame.Navigate(new ProductPage(user));
                LoginBox.Text = "";
                PasswordBox.Text = "";
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль", "Ошибка");
                GenerateCaptcha(); // Генерируем новую капчу при ошибке

                // Блокируем кнопку на 10 секунд
                BlockLoginButton(10);
            }
        }



        private void RefreshCaptchaButton_Click_1(object sender, RoutedEventArgs e)
        {
            GenerateCaptcha();
        }
    }
}
