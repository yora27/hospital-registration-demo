using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
namespace Hosreg1
{
    public partial class EditPatientWindow : Window
    {
        private readonly Patient _patient;
        private readonly bool _isNewPatient;

        public EditPatientWindow(Patient patient = null)
        {
            InitializeComponent();

            if (patient == null)
            {
                _isNewPatient = true;
                _patient = new Patient
                {
                    Id = Database.Patients.Count > 0 ? Database.Patients.Max(p => p.Id) + 1 : 1,
                    Comments = new System.Collections.Generic.List<string>()
                };
            }
            else
            {
                _isNewPatient = false;
                _patient = patient;
            }

            LoadPatientData();
        }

        private void LoadPatientData()
        {
            NameTextBox.Text = _patient.Name;
            PhoneTextBox.Text = _patient.Phone;
            BirthDatePicker.SelectedDate = _patient.BirthDate;
            AddressTextBox.Text = _patient.Address;
            LoginTextBox.Text = _patient.Login;

            // Якщо це новий пацієнт, генеруємо логін
            if (_isNewPatient)
            {
                LoginTextBox.Text = $"patient{_patient.Id}";
                PasswordBox.Password = "password123"; // Початковий пароль
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Будь ласка, введіть ПІБ пацієнта", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(LoginTextBox.Text))
            {
                MessageBox.Show("Будь ласка, введіть логін", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Перевірка унікальності логіна
            var existingPatient = Database.Patients.FirstOrDefault(p =>
                p.Login == LoginTextBox.Text && p.Id != _patient.Id);

            if (existingPatient != null)
            {
                MessageBox.Show("Такий логін вже існує", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Оновлюємо дані пацієнта
            _patient.Name = NameTextBox.Text;
            _patient.Phone = PhoneTextBox.Text;
            _patient.BirthDate = BirthDatePicker.SelectedDate;
            _patient.Address = AddressTextBox.Text;
            _patient.Login = LoginTextBox.Text;

            if (!string.IsNullOrEmpty(PasswordBox.Password))
            {
                _patient.Password = PasswordBox.Password;
            }

            // Якщо це новий пацієнт, додаємо його до бази даних
            if (_isNewPatient)
            {
                Database.Patients.Add(_patient);
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}