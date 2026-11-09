using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CarServiceApp
{
    public partial class MainWindow : Window
    {
        private readonly int _currentClientId;

        public List<ServiceItem> CatalogServices { get; set; }
        public List<ServiceOrder> AllClientOrders { get; set; }

        // Конструктор приймає ID залогіненого користувача (за замовчуванням 1)
        public MainWindow(int clientId = 1)
        {
            InitializeComponent();
            _currentClientId = clientId;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // 1. Отримання каталогу послуг з БД сервера (Команда 4#)
                LoadCatalogFromDatabase();

                // 2. Отримання історії та активних замовлень з БД сервера (Команда 3#)
                LoadOrdersFromDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження даних: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCatalogFromDatabase()
        {
            string response = ServerClient.SendRequest("4#");

            if (response.StartsWith("OK"))
            {
                CatalogServices = new List<ServiceItem>();
                string[] items = response.Split(';');

                // Перебираємо елементи, пропускаючи першу мітку "OK"
                for (int i = 1; i < items.Length; i++)
                {
                    string[] parts = items[i].Split('#');
                    if (parts.Length >= 5)
                    {
                        CatalogServices.Add(new ServiceItem
                        {
                            Title = parts[0],
                            Category = parts[1],
                            Price = decimal.TryParse(parts[2], out decimal p) ? p : 0,
                            Description = parts[3],
                            Duration = parts[4]
                        });
                    }
                }

                DgCatalog.ItemsSource = CatalogServices;
            }
        }

        private void LoadOrdersFromDatabase()
        {
            string response = ServerClient.SendRequest($"3#{_currentClientId}");

            if (response.StartsWith("OK"))
            {
                AllClientOrders = new List<ServiceOrder>();
                string[] items = response.Split(';');

                for (int i = 1; i < items.Length; i++)
                {
                    string[] parts = items[i].Split('#');
                    if (parts.Length >= 5)
                    {
                        AllClientOrders.Add(new ServiceOrder
                        {
                            Id = int.TryParse(parts[0], out int id) ? id : 0,
                            ServiceType = parts[1],
                            Price = decimal.TryParse(parts[2], out decimal p) ? p : 0,
                            Status = parts[3],
                            OrderDate = DateTime.TryParse(parts[4], out DateTime d) ? d : DateTime.Now
                        });
                    }
                }

                // 2. Фільтруємо лише активні замовлення
                DgActiveOrders.ItemsSource = AllClientOrders.Where(o => o.Status != "Завершено").ToList();

              
            }
        }

        private void BtnBookFromCatalog_Click(object sender, RoutedEventArgs e)
        {
            CreateBookingWindow bookingWindow = new CreateBookingWindow(_currentClientId);
            bookingWindow.ShowDialog();

            // Оновлюємо дані після створення нового запису
            LoadData();
        }

        private void BtnNewBooking_Click(object sender, RoutedEventArgs e)
        {
            CreateBookingWindow bookingWindow = new CreateBookingWindow(_currentClientId);
            bookingWindow.ShowDialog();

            // Оновлюємо дані після створення нового запису
            LoadData();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}