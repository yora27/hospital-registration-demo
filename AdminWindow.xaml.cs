using LoginApp;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Text.Json;

namespace Hosreg1
{

    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();
            LoadAdminData();
        }

        private void LoadAdminData()
        {
            RefreshPatientsList();
            UpdateStatistics();
            LoadAppointments();
        }

        private void RefreshPatientsList()
        {
            PatientsList.Items.Clear();
            foreach (var patient in Database.Patients.Where(p => p.Id != 0)) // Skip admin
            {
                var item = new ListViewItem();
                item.Content = patient;
                item.Tag = patient;
                PatientsList.Items.Add(item);
            }

            // Set up the columns with proper bindings
            var gridView = PatientsList.View as GridView;
            if (gridView != null)
            {
                gridView.Columns.Clear();
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "ПІБ",
                    Width = 200,
                    DisplayMemberBinding = new Binding("Name")
                });
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "Дата народження",
                    Width = 120,
                    DisplayMemberBinding = new Binding("BirthDate")
                    {
                        StringFormat = "dd.MM.yyyy"
                    }
                });
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "Адреса",
                    Width = 200,
                    DisplayMemberBinding = new Binding("Address")
                });
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "Телефон",
                    Width = 150,
                    DisplayMemberBinding = new Binding("Phone")
                });
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "Статус",
                    Width = 100,
                    DisplayMemberBinding = new Binding(".")
                    {
                        Converter = new PatientStatusConverter()
                    }
                });
            }
        }

        private void LoadAppointments()
        {
            AllAppointmentsList.Items.Clear();
            foreach (var appointment in Database.Appointments)
            {
                var item = new ListViewItem();
                item.Content = appointment;
                item.Tag = appointment;
                AllAppointmentsList.Items.Add(item);
            }

            // Set up the columns with proper bindings
            var gridView = AllAppointmentsList.View as GridView;
            if (gridView != null)
            {
                gridView.Columns.Clear();
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "Дата",
                    Width = 100,
                    DisplayMemberBinding = new Binding("DateTime")
                    {
                        StringFormat = "dd.MM.yyyy"
                    }
                });
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "Час",
                    Width = 100,
                    DisplayMemberBinding = new Binding("DateTime")
                    {
                        StringFormat = "HH:mm"
                    }
                });
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "Пацієнт",
                    Width = 200,
                    DisplayMemberBinding = new Binding("Patient.Name")
                });
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "Лікар",
                    Width = 200,
                    DisplayMemberBinding = new Binding("Doctor.Name")
                });
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "Статус",
                    Width = 100,
                    DisplayMemberBinding = new Binding { Path = new PropertyPath("IsActive") }
                });
            }
        }

        private void UpdateStatistics()
        {
            TotalPatientsCount.Text = Database.Patients.Count(p => p.Id != 0).ToString();
            TodayAppointmentsCount.Text = Database.Appointments
                .Count(a => a.DateTime.Date == DateTime.Today).ToString();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }

        private void AddPatient_Click(object sender, RoutedEventArgs e)
        {
            var newPatient = new Patient
            {
                Id = Database.Patients.Max(p => p.Id) + 1,
                Name = "Новий пацієнт",
                Login = $"patient{Database.Patients.Count + 1}",
                Password = "password123",
                Comments = new System.Collections.Generic.List<string>()
            };

            Database.Patients.Add(newPatient);
            RefreshPatientsList();
            UpdateStatistics();
            MessageBox.Show("Новий пацієнт доданий успішно!");
        }

        private void DeletePatient_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = PatientsList.SelectedItem as ListViewItem;
            if (selectedItem?.Tag is Patient selectedPatient)
            {
                var result = MessageBox.Show(
                    "Ви впевнені, що хочете видалити цього пацієнта?",
                    "Підтвердження видалення",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Database.Patients.Remove(selectedPatient);

                    // Видаляємо пов'язані записи
                    var appointmentsToRemove = Database.Appointments
                        .Where(a => a.Patient.Id == selectedPatient.Id)
                        .ToList();

                    foreach (var appointment in appointmentsToRemove)
                    {
                        Database.Appointments.Remove(appointment);
                    }

                    RefreshPatientsList();
                    UpdateStatistics();
                    LoadAppointments();
                    MessageBox.Show("Пацієнт видалений успішно!");
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть пацієнта для видалення.");
            }
        }
        private void EditPatient_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = PatientsList.SelectedItem as ListViewItem;
            if (selectedItem?.Tag is Patient selectedPatient)
            {
                var editWindow = new EditPatientWindow(selectedPatient);
                editWindow.Owner = this;

                if (editWindow.ShowDialog() == true)
                {
                    RefreshPatientsList();
                    UpdateStatistics();
                    MessageBox.Show("Дані пацієнта оновлено успішно!", "Успіх",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть пацієнта для редагування.",
                    "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AddAppointment_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = PatientsList.SelectedItem as ListViewItem;
            Appointment newAppointment = null;

            if (selectedItem?.Tag is Patient selectedPatient)
            {
                newAppointment = new Appointment
                {
                    Patient = selectedPatient,
                    IsActive = true // Встановлюємо запис як активний за замовчуванням
                };
            }

            var appointmentWindow = new EditAppointmentWindow(newAppointment);
            appointmentWindow.Owner = this;

            if (appointmentWindow.ShowDialog() == true)
            {
                // Оновлюємо всі списки після створення запису
                LoadAppointments();
                RefreshPatientsList(); // Додано оновлення списку пацієнтів
                UpdateStatistics();
                MessageBox.Show("Запис створено успішно!", "Успіх",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    public class PatientStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Patient patient)
            {
                // Спрощуємо перевірку - дивимось лише на наявність активного запису
                var hasActiveAppointment = Database.Appointments
                    .Any(a => a.Patient.Id == patient.Id &&
                             a.IsActive);

                return hasActiveAppointment ? "Записаний" : "Не записаний";
            }
            return "Невідомо";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DoctorStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null ? "Записаний" : "Не записаний";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

