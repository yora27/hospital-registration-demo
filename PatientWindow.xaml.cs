using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Text.Json;
using LoginApp;
using Hosreg1;
using System.Text.Json.Serialization;
using System.Globalization;
using System.Windows.Data;

namespace Hosreg1
{
    public partial class PatientWindow : Window
    {
        private readonly TcpClientService _tcpClient;
        private Patient currentPatient;

        public PatientWindow(Patient patient)
        {
            InitializeComponent();
            _tcpClient = new TcpClientService();
            currentPatient = patient;
            InitializeConnectionAndLoadData();
        }

        private async void InitializeConnectionAndLoadData()
        {
            try
            {
                await _tcpClient.ConnectAsync();
                await LoadPatientDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка підключення: {ex.Message}", "Помилка з'єднання",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadPatientDataAsync()
        {
            try
            {
                var request = new
                {
                    command = "GET_PATIENT_DATA",
                    patientId = currentPatient.Id
                };

                var response = await _tcpClient.SendRequestAsync<PatientDataResponse>(request);

                if (response == null)
                {
                    MessageBox.Show("Не вдалося отримати дані пацієнта", "Помилка завантаження",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Оновлюємо UI з отриманими даними
                txtUserName.Text = response.Name;
                txtFullName.Text = response.Name;
                dateBirth.SelectedDate = response.BirthDate;
                txtPhone.Text = response.Phone;

                // Завантаження списку лікарів
                var doctorsRequest = new { command = "GET_DOCTORS" };
                var doctorsResponse = await _tcpClient.SendRequestAsync<List<Doctor>>(doctorsRequest);
                DoctorsList.ItemsSource = doctorsResponse;
                ReviewDoctorsList.ItemsSource = doctorsResponse;

                // Оновлюємо списки прийомів та відгуків
                RefreshAppointments();
                RefreshReviews();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження даних: {ex.Message}");
            }
        }

        private async void RefreshAppointments()
        {
            try
            {
                var request = new
                {
                    command = "GET_APPOINTMENTS",
                    patientId = currentPatient.Id
                };

                var appointments = await _tcpClient.SendRequestAsync<List<AppointmentResponse>>(request);

                if (appointments == null || appointments.Count == 0)
                {
                    MessageBox.Show("Записів на прийом не знайдено", "Порожній список",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    AppointmentsList.ItemsSource = appointments;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка оновлення списку прийомів: {ex.Message}", "Помилка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void RefreshReviews()
        {
            try
            {
                var request = new
                {
                    command = "GET_REVIEWS",
                    patientId = currentPatient.Id
                };

                var reviews = await _tcpClient.SendRequestAsync<List<ReviewResponse>>(request);
                ReviewsList.ItemsSource = reviews;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка оновлення відгуків: {ex.Message}");
            }
        }


        private async void AddReview_Click(object sender, RoutedEventArgs e)
        {
            if (ReviewDoctorsList.SelectedItem is Doctor selectedDoctor &&
                !string.IsNullOrWhiteSpace(NewReviewText.Text))
            {
                try
                {
                    var request = new
                    {
                        command = "CREATE_REVIEW",
                        patientId = currentPatient.Id,
                        doctorId = selectedDoctor.Id,
                        comment = NewReviewText.Text,
                        date = DateTime.Now
                    };

                    var response = await _tcpClient.SendRequestAsync<ReviewResponse>(request);
                    RefreshReviews();
                    NewReviewText.Clear();
                    ReviewDoctorsList.SelectedIndex = -1;
                    MessageBox.Show("Відгук додано успішно!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка додавання відгуку: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть лікаря та введіть текст відгуку");
            }
        }

        private async void SaveProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var request = new
                {
                    command = "UPDATE_PATIENT",
                    patientId = currentPatient.Id,
                    name = txtFullName.Text,
                    birthDate = dateBirth.SelectedDate,
                    phone = txtPhone.Text
                };

                var response = await _tcpClient.SendRequestAsync<PatientUpdateResponse>(request);
                txtUserName.Text = txtFullName.Text;
                MessageBox.Show("Профіль успішно оновлено!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка оновлення профілю: {ex.Message}");
            }
        }

        private async void UpdateAvailableTimeSlots()
        {
            if (DoctorsList.SelectedItem is Doctor selectedDoctor && AppointmentDate.SelectedDate.HasValue)
            {
                try
                {
                    var request = new
                    {
                        command = "GET_AVAILABLE_TIMESLOTS",
                        doctorId = selectedDoctor.Id,
                        date = AppointmentDate.SelectedDate.Value
                    };

                    var timeSlots = await _tcpClient.SendRequestAsync<List<TimeSlotResponse>>(request);
                    TimeSlotsList.ItemsSource = timeSlots.Select(ts => new TimeSlotViewModel
                    {
                        DisplayText = ts.StartTime.ToString("HH:mm"),
                        StartTime = ts.StartTime,
                        IsAvailable = ts.IsAvailable
                    }).ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка отримання доступних часових слотів: {ex.Message}");
                }
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Створюємо нове вікно входу
            var loginWindow = new 
                
                Window();
            loginWindow.Show();

            // Закриваємо поточне вікно
            this.Close();
        }

        private async void ShowDoctorReviews_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var request = new
                {
                    command = "GET_ALL_DOCTOR_REVIEWS"
                };

                var reviews = await _tcpClient.SendRequestAsync<List<ReviewResponse>>(request);

                // Create and show a new window with all doctor reviews
                var reviewsWindow = new DoctorReviewsWindow(reviews);
                reviewsWindow.Owner = this;
                reviewsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні відгуків: {ex.Message}");
            }
        }
        private async void MakeAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (DoctorsList.SelectedItem is Doctor selectedDoctor &&
                AppointmentDate.SelectedDate.HasValue &&
                TimeSlotsList.SelectedItem is TimeSlotViewModel selectedTimeSlot)
            {
                try
                {
                    var appointmentDateTime = selectedTimeSlot.StartTime;

                    if (!selectedTimeSlot.IsAvailable)
                    {
                        MessageBox.Show("Обраний час вже зайнятий. Будь ласка, оберіть інший час.");
                        return;
                    }

                    var request = new
                    {
                        command = "CREATE_APPOINTMENT",
                        patientId = currentPatient.Id,
                        doctorId = selectedDoctor.Id,
                        dateTime = appointmentDateTime
                    };

                    var response = await _tcpClient.SendRequestAsync<AppointmentResponse>(request);

                    if (response.Success)
                    {
                        MessageBox.Show("Запис на прийом успішно створено!");
                        RefreshAppointments(); // Оновлюємо список прийомів
                                               // Очищаємо поля форми
                        DoctorsList.SelectedIndex = -1;
                        AppointmentDate.SelectedDate = null;
                        TimeSlotsList.ItemsSource = null;
                    }
                    else
                    {
                        MessageBox.Show($"Помилка створення запису: {response.Message}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, заповніть усі поля");
            }
        }


    
        private void DoctorsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AppointmentDate.SelectedDate.HasValue)
            {
                UpdateAvailableTimeSlots();
            }
        }

        private void AppointmentDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DoctorsList.SelectedItem != null)
            {
                UpdateAvailableTimeSlots();
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _tcpClient.Disconnect();
        }
    }



    // Response classes

    public class AppointmentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime DateTime { get; set; }
        public DoctorDetails Doctor { get; set; }
        public string Status
        {
            get
            {
                return DateTime > System.DateTime.Now ? "Активний" : "Завершений";
            }
        }
    }
    public class TimeSlotResponse
    {
        public DateTime StartTime { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class TimeSlotViewModel
    {
        public string DisplayText { get; set; }
        public DateTime StartTime { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class AppointmentDetails
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime DateTime { get; set; }
        public DoctorDetails Doctor { get; set; }
        public string Status
        {
            get
            {
                return DateTime > System.DateTime.Now ? "Активний" : "Завершений";
            }
        }
    }

    public class DoctorDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Specialization { get; set; }
    }
    public class PatientDataResponse
    {
        public string Name { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Phone { get; set; }
    }


    public class ReviewResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("doctor")]
        public Doctor Doctor { get; set; }
    }

    public class PatientUpdateResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
    public class BoolToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "Активний" : "Завершений";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}