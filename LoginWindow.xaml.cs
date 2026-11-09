using System;
using System.Windows;

namespace CarServiceApp
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = TxtLogin.Text.Trim();
                string password = TxtPassword.Password;

                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                {
                    throw new Exception("Будь ласка, заповніть усі поля!");
                }

                // Вхід як Адміністратор
                if (login.ToLower() == "admin" && password == "admin")
                {
                    AdminWindow adminWindow = new AdminWindow();
                    adminWindow.Show();
                    this.Close();
                    return;
                }

                // Вхід як Клієнт (1#login#password)
                string request = $"1#{login}#{password}";
                string response = ServerClient.SendRequest(request);
                string[] parts = response.Split('#');

                if (parts[0] == "OK")
                {
                    // ВИПРАВЛЕНО: Зчитати справжній user_id з відповіді сервера (parts[1])
                    int clientId = (parts.Length > 1 && int.TryParse(parts[1], out int id)) ? id : 1;

                    MessageBox.Show("Авторизація успішна!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Передаємо ID конкретного користувача у MainWindow
                    MainWindow clientWindow = new MainWindow(clientId);
                    clientWindow.Show();
                    this.Close();
                }
                else
                {
                    string errorMsg = parts.Length > 1 ? parts[1] : "Невірний логін або пароль!";
                    MessageBox.Show(errorMsg, "Помилка авторизації", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.ShowDialog();
        }
    }
}