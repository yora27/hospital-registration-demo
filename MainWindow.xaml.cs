// MainWindow.xaml.cs
using Hosreg1;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Text.Json;

namespace LoginApp
{
    public partial class MainWindow : Window
    {
        private readonly TcpClientService _tcpClient;

        public MainWindow()
        {
            InitializeComponent();
            _tcpClient = new TcpClientService();
            ConnectToServer();
        }

        private async void ConnectToServer()
        {
            try
            {
                await _tcpClient.ConnectAsync();
                Console.WriteLine("Підключено до сервера");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка підключення до сервера: {ex.Message}");
            }
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var request = new
                {
                    command = "LOGIN",
                    login = txtUsername.Text,
                    password = txtPassword.Password
                };

                var response = await _tcpClient.SendRequestAsync<JsonElement>(request);
                bool isAuthenticated = response.GetProperty("IsAuthenticated").GetBoolean();

                if (isAuthenticated)
                {
                    int id = response.GetProperty("Id").GetInt32();
                    if (id == 0) // Admin
                    {
                        AdminWindow adminWindow = new AdminWindow();
                        adminWindow.Show();
                    }
                    else // Patient
                    {
                        // Create a Patient object with the ID
                        var patient = new Patient { Id = id };
                        PatientWindow patientWindow = new PatientWindow(patient);
                        patientWindow.Show();
                    }
                    this.Close();
                }
                else
                {
                    txtError.Text = "Неправильний логін або пароль!";
                }
            }
            catch (Exception ex)
            {
                txtError.Text = $"Помилка: {ex.Message}";
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _tcpClient?.Disconnect();
        }

        private void txtUsername_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
        }
    }
}