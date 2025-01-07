using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Hosreg1
{
    public partial class EditAppointmentWindow : Window
    {
        private readonly Appointment _appointment;
        private readonly bool _isNewAppointment;

        public EditAppointmentWindow(Appointment appointment = null)
        {
            InitializeComponent();

            if (appointment == null)
            {
                _isNewAppointment = true;
                _appointment = new Appointment
                {
                    Id = Database.Appointments.Count > 0 ?
                         Database.Appointments.Max(a => a.Id) + 1 : 1
                };
            }
            else
            {
                _isNewAppointment = false;
                _appointment = appointment;
            }

            LoadComboBoxes();
            LoadAppointmentData();
        }

        private void LoadComboBoxes()
        {
            // Завантаження пацієнтів
            PatientComboBox.ItemsSource = Database.Patients.Where(p => p.Id != 0);

            // Завантаження лікарів
            DoctorComboBox.ItemsSource = Database.Doctors;

            // Встановлення поточної дати
            AppointmentDatePicker.SelectedDate = DateTime.Today;
        }

        private void LoadAppointmentData()
        {
            if (!_isNewAppointment)
            {
                PatientComboBox.SelectedItem = _appointment.Patient;
                DoctorComboBox.SelectedItem = _appointment.Doctor;
                AppointmentDatePicker.SelectedDate = _appointment.DateTime.Date;
                UpdateTimeSlots();
                TimeSlotComboBox.SelectedValue = _appointment.DateTime.TimeOfDay;
            }
        }

        private void UpdateTimeSlots()
        {
            TimeSlotComboBox.Items.Clear();
            var selectedDoctor = DoctorComboBox.SelectedItem as Doctor;
            var selectedDate = AppointmentDatePicker.SelectedDate;

            if (selectedDoctor != null && selectedDate.HasValue)
            {
                var availableSlots = selectedDoctor.AvailableTimeSlots
                    .Where(slot => slot.StartTime.Date == selectedDate.Value.Date && slot.IsAvailable)
                    .OrderBy(slot => slot.StartTime);

                foreach (var slot in availableSlots)
                {
                    TimeSlotComboBox.Items.Add(new ComboBoxItem
                    {
                        Content = slot.StartTime.ToString("HH:mm"),
                        Tag = slot
                    });
                }
            }
        }

        private void DoctorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTimeSlots();
        }

        private void AppointmentDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTimeSlots();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (PatientComboBox.SelectedItem == null)
            {
                MessageBox.Show("Виберіть пацієнта", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DoctorComboBox.SelectedItem == null)
            {
                MessageBox.Show("Виберіть лікаря", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TimeSlotComboBox.SelectedItem == null)
            {
                MessageBox.Show("Виберіть час прийому", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedTimeSlot = (TimeSlot)((ComboBoxItem)TimeSlotComboBox.SelectedItem).Tag;

            _appointment.Patient = (Patient)PatientComboBox.SelectedItem;
            _appointment.Doctor = (Doctor)DoctorComboBox.SelectedItem;
            _appointment.DateTime = selectedTimeSlot.StartTime;

            if (_isNewAppointment)
            {
                Database.Appointments.Add(_appointment);
            }

            if (!string.IsNullOrWhiteSpace(CommentTextBox.Text))
            {
                _appointment.Patient.AddComment(CommentTextBox.Text);
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