using System;
using System.Collections.Generic;
using System.Windows;

namespace CarServiceApp
{
    public partial class AdminWindow : Window
    {
        public List<ServiceOrder> AllOrders { get; set; } = new List<ServiceOrder>();
        public List<ServiceItem> AllServices { get; set; } = new List<ServiceItem>();

        public AdminWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            LoadOrdersFromDatabase();
            LoadServicesFromDatabase();
        }

        // 1. Зчитати всі активні замовлення з БД (Команда 8#)
        private void LoadOrdersFromDatabase()
        {
            try
            {
                string response = ServerClient.SendRequest("8#");

                if (response.StartsWith("OK"))
                {
                    AllOrders = new List<ServiceOrder>();
                    string[] items = response.Split(';');

                    for (int i = 1; i < items.Length; i++)
                    {
                        string[] parts = items[i].Split('#');
                        if (parts.Length >= 8)
                        {
                            AllOrders.Add(new ServiceOrder
                            {
                                Id = int.TryParse(parts[0], out int id) ? id : 0,
                                ClientName = parts[1],
                                CarModel = parts[2],
                                CarNumber = parts[3],
                                ServiceType = parts[4],
                                Price = decimal.TryParse(parts[5], out decimal p) ? p : 0,
                                Status = parts[6],
                                OrderDate = DateTime.TryParse(parts[7], out DateTime d) ? d : DateTime.Now
                            });
                        }
                    }

                    DgAdminOrders.ItemsSource = null;
                    DgAdminOrders.ItemsSource = AllOrders;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження замовлень: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 2. Отримання каталогa послуг (Команда 4#)
        private void LoadServicesFromDatabase()
        {
            try
            {
                string response = ServerClient.SendRequest("4#");

                if (response.StartsWith("OK"))
                {
                    AllServices = new List<ServiceItem>();
                    string[] items = response.Split(';');

                    for (int i = 1; i < items.Length; i++)
                    {
                        string[] parts = items[i].Split('#');
                        if (parts.Length >= 5)
                        {
                            AllServices.Add(new ServiceItem
                            {
                                Title = parts[0],
                                Category = parts[1],
                                Price = decimal.TryParse(parts[2], out decimal p) ? p : 0,
                                Description = parts[3],
                                Duration = parts[4]
                            });
                        }
                    }

                    DgServicesAdmin.ItemsSource = null;
                    DgServicesAdmin.ItemsSource = AllServices;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження послуг: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 3. Зміна статусу замовлення (В черзі -> Прийнято -> Виконано)
        private void BtnUpdateStatus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedOrder = DgAdminOrders.SelectedItem as ServiceOrder;
                if (selectedOrder == null)
                {
                    MessageBox.Show("Будь ласка, виберіть заявку зі списку!", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Послідовна зміна статусу
                string newStatus = selectedOrder.Status switch
                {
                    "В черзі" => "Прийнято",
                    "Прийнято" => "Виконано",
                    "Виконано" => "Виконано",
                    _ => "Прийнято"
                };

                if (selectedOrder.Status == "Виконано")
                {
                    MessageBox.Show("Заявка вже має статус 'Виконано'. Сформуйте рахунок, щоб завершити її!", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Надсилаємо команду 7#id#newStatus на сервер
                string response = ServerClient.SendRequest($"7#{selectedOrder.Id}#{newStatus}");

                if (response.StartsWith("OK"))
                {
                    MessageBox.Show($"Статус заявки №{selectedOrder.Id} оновлено на: '{newStatus}'", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadOrdersFromDatabase(); // Перезавантажуємо список із сервера
                }
                else
                {
                    MessageBox.Show($"Помилка сервера: {response}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 4. Сформувати рахунок та видалити з активного списку (Статус -> Завершено)
        private void BtnGenerateInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedOrder = DgAdminOrders.SelectedItem as ServiceOrder;
                if (selectedOrder == null)
                {
                    MessageBox.Show("Виберіть заявку для формування рахунку!", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 1. Показуємо чек
                string invoiceText = $"--- РАХУНОК ЗА РЕМОНТ №{selectedOrder.Id} ---\n\n" +
                                     $"Клієнт: {selectedOrder.ClientName}\n" +
                                     $"Автомобіль: {selectedOrder.CarModel} ({selectedOrder.CarNumber})\n" +
                                     $"Виконана робота: {selectedOrder.ServiceType}\n" +
                                     $"Загальна вартість: {selectedOrder.Price} грн\n" +
                                     $"Дата формування: {DateTime.Now:dd.MM.yyyy HH:mm}";

                MessageBox.Show(invoiceText, "Чек сформовано", MessageBoxButton.OK, MessageBoxImage.Information);

                // 2. Встановлюємо статус 'Завершено' в БД
                string response = ServerClient.SendRequest($"7#{selectedOrder.Id}#Завершено");

                if (response.StartsWith("OK"))
                {
                    // 3. Перезавантажуємо таблицю — замовлення зі статусом 'Завершено' зникає з адмінки
                    LoadOrdersFromDatabase();
                }
                else
                {
                    MessageBox.Show($"Помилка при збереженні: {response}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 5. Додавання послуги в каталог
        private void BtnAddService_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TxtServiceName.Text) ||
                    string.IsNullOrWhiteSpace(TxtCategory.Text) ||
                    string.IsNullOrWhiteSpace(TxtPrice.Text))
                {
                    MessageBox.Show("Заповніть назву, категорію та ціну послуги!", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string name = TxtServiceName.Text.Trim();
                string category = TxtCategory.Text.Trim();
                string price = TxtPrice.Text.Trim();
                string desc = string.IsNullOrWhiteSpace(TxtDescription.Text) ? "Немає опису" : TxtDescription.Text.Trim();
                string duration = string.IsNullOrWhiteSpace(TxtDuration.Text) ? "1 год" : TxtDuration.Text.Trim();

                string request = $"6#{name}#{category}#{price}#{desc}#{duration}";
                string response = ServerClient.SendRequest(request);

                if (response.StartsWith("OK"))
                {
                    MessageBox.Show("Нову послугу успішно додано в базу даних!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                    TxtServiceName.Clear();
                    TxtCategory.Clear();
                    TxtPrice.Clear();
                    TxtDescription.Clear();
                    TxtDuration.Clear();

                    LoadServicesFromDatabase();
                }
                else
                {
                    MessageBox.Show($"Помилка сервера: {response}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}