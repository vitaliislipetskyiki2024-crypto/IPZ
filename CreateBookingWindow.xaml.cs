using System;
using System.Collections.Generic;
using System.Windows;

namespace CarServiceApp
{
    public partial class CreateBookingWindow : Window
    {
        private readonly int _clientId;
        private List<ServiceItem> _services = new List<ServiceItem>();

        // Приймає ID клієнта
        public CreateBookingWindow(int clientId = 1)
        {
            InitializeComponent();
            _clientId = clientId;
            DpOrderDate.SelectedDate = DateTime.Now;
            LoadServicesFromDatabase();
        }

        // 1. Отримання послуг з БД сервера (Команда 4#)
        private void LoadServicesFromDatabase()
        {
            try
            {
                string response = ServerClient.SendRequest("4#");
                if (response.StartsWith("OK"))
                {
                    _services = new List<ServiceItem>();
                    string[] items = response.Split(';');

                    for (int i = 1; i < items.Length; i++)
                    {
                        string[] parts = items[i].Split('#');
                        if (parts.Length >= 5)
                        {
                            _services.Add(new ServiceItem
                            {
                                Title = parts[0],
                                Category = parts[1],
                                Price = decimal.TryParse(parts[2], out decimal p) ? p : 0,
                                Description = parts[3],
                                Duration = parts[4]
                            });
                        }
                    }

                    // Оновлюємо джерело даних ComboBox
                    CbServices.ItemsSource = null;
                    CbServices.ItemsSource = _services;

                    if (_services.Count > 0)
                    {
                        CbServices.SelectedIndex = 0; // Початковий вибір
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження каталогу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 2. Натискання кнопки "Підтвердити запис" (Команда 5#)
        private void BtnSubmitOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Беремо саме вибраний елемент із випадаючого списку
                var selectedService = CbServices.SelectedItem as ServiceItem;

                if (selectedService == null)
                {
                    MessageBox.Show("Будь ласка, оберіть послугу зі списку!", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Формат запиту: 5#client_id#service_type#price
                string request = $"5#{_clientId}#{selectedService.Title}#{selectedService.Price}";
                string response = ServerClient.SendRequest(request);

                if (response.StartsWith("OK"))
                {
                    MessageBox.Show($"Заявку на послугу '{selectedService.Title}' успішно створено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show($"Помилка сервера: {response}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка створення запису: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}