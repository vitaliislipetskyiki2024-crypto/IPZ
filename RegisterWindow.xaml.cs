using System;
using System.Windows;

namespace CarServiceApp
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void BtnRegisterConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = TxtLogin.Text.Trim();
                string password = TxtPassword.Password;
                string name = TxtName.Text.Trim();
                string phone = TxtPhone.Text.Trim();
                string carModel = TxtCarModel.Text.Trim();
                string carNumber = TxtCarNumber.Text.Trim();

                if (string.IsNullOrWhiteSpace(login) ||
                    string.IsNullOrWhiteSpace(password) ||
                    string.IsNullOrWhiteSpace(name) ||
                    string.IsNullOrWhiteSpace(carModel) ||
                    string.IsNullOrWhiteSpace(carNumber))
                {
                    throw new Exception("Заповніть усі обов'язкові поля!");
                }

                // Формуємо запит №2 для TCP-сервера: 2#login#pass#name#second_name#phone#car_model#car_number
                string request = $"2#{login}#{password}#{name}#Клієнт#{phone}#{carModel}#{carNumber}";

                string response = ServerClient.SendRequest(request);
                string[] parts = response.Split('#');

                if (parts[0] == "OK")
                {
                    MessageBox.Show(parts.Length > 1 ? parts[1] : "Реєстрація пройшла успішно!", "Успіх",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    string errorMsg = parts.Length > 1 ? parts[1] : "Помилка реєстрації на сервері";
                    MessageBox.Show(errorMsg, "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка введення", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}