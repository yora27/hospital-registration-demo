using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using Dapper;
using System.Linq;

namespace HosregServer1
{
    public class DatabaseService
    {
        private readonly string _connectionString = @"Server=jamal;Database=HospitalDB;Trusted_Connection=True;";

        public async Task<string> AuthenticateUser(string login, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(
                    "SELECT Id, Login, Password FROM Patients WHERE Login = @Login AND Password = @Password",
                    connection);

                command.Parameters.AddWithValue("@Login", login);
                command.Parameters.AddWithValue("@Password", password);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var user = new
                    {
                        Id = reader.GetInt32(0),
                        Login = reader.GetString(1),
                        IsAuthenticated = true
                    };
                    return JsonSerializer.Serialize(user);
                }
                return JsonSerializer.Serialize(new { IsAuthenticated = false });
            }
        }

        public async Task<bool> TestConnection()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("SELECT 1", connection))
                    {
                        await command.ExecuteScalarAsync();
                    }
                    Console.WriteLine("Успішне підключення до бази даних!");
                    return true;
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Помилка підключення до бази даних: {ex.Message}");
                Console.WriteLine($"Код помилки: {ex.Number}");
                return false;
            }
        }

        public async Task<string> CreateAppointment(int patientId, int doctorId, DateTime dateTime)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Перевіряємо, чи не зайнятий цей час
                var existingAppointment = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    @"SELECT Id FROM Appointments 
              WHERE DoctorId = @DoctorId 
              AND DateTime = @DateTime",
                    new { DoctorId = doctorId, DateTime = dateTime }
                );

                if (existingAppointment != null)
                {
                    return JsonSerializer.Serialize(new
                    {
                        Success = false,
                        Message = "Цей час вже зайнятий. Будь ласка, оберіть інший час."
                    });
                }

                // Створюємо новий запис
                await connection.ExecuteAsync(
                    @"INSERT INTO Appointments (PatientId, DoctorId, DateTime)
              VALUES (@PatientId, @DoctorId, @DateTime)",
                    new { PatientId = patientId, DoctorId = doctorId, DateTime = dateTime }
                );

                return JsonSerializer.Serialize(new
                {
                    Success = true,
                    Message = "Запис успішно створено"
                });
            }
        }

        public async Task<string> GetAllPatients()
        {
            var patients = new List<object>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT Id, Name, Login FROM Patients WHERE Id != 0", connection);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    patients.Add(new
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Login = reader.GetString(2)
                    });
                }
            }
            return JsonSerializer.Serialize(patients);


        }
        public async Task<string> GetPatientData(int patientId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var patient = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT Name, BirthDate, Phone FROM Patients WHERE Id = @PatientId",
                    new { PatientId = patientId }
                );

                if (patient == null)
                {
                    Console.WriteLine($"Пацієнт з ID {patientId} не знайдений");
                    return JsonSerializer.Serialize(new { error = "Patient not found" });
                }

                return JsonSerializer.Serialize(new
                {
                    Name = patient.Name,
                    BirthDate = patient.BirthDate,
                    Phone = patient.Phone
                });
            }
        }

        public async Task<string> GetAllDoctors()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var doctors = await connection.QueryAsync<dynamic>(
                    "SELECT Id, Name, Specialization FROM Doctors"
                );

                if (!doctors.Any())
                {
                    Console.WriteLine("Лікарів не знайдено");
                    return JsonSerializer.Serialize(new List<object>());
                }

                return JsonSerializer.Serialize(doctors);
            }
        }




        public async Task<string> GetPatientReviews(int patientId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var reviews = await connection.QueryAsync<dynamic>(
                    @"SELECT r.Id, r.Comment, r.Date,
                      d.Id as DoctorId, d.Name as DoctorName, d.Specialization
                      FROM Reviews r
                      JOIN Doctors d ON r.DoctorId = d.Id
                      WHERE r.PatientId = @PatientId
                      ORDER BY r.Date DESC",
                    new { PatientId = patientId }
                );

                var formattedReviews = reviews.Select(r => new
                {
                    Id = r.Id,
                    Comment = r.Comment,
                    Date = r.Date,
                    Doctor = new
                    {
                        Id = r.DoctorId,
                        Name = r.DoctorName,
                        Specialization = r.Specialization
                    }
                });

                return JsonSerializer.Serialize(formattedReviews);
            }
        }

        public async Task<string> CreateReview(int patientId, int doctorId, string comment, DateTime date)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(
                    @"INSERT INTO Reviews (PatientId, DoctorId, Comment, Date)
                      VALUES (@PatientId, @DoctorId, @Comment, @Date)",
                    new { PatientId = patientId, DoctorId = doctorId, Comment = comment, Date = date }
                );

                return JsonSerializer.Serialize(new { success = true });
            }
        }

        public async Task<string> UpdatePatient(int patientId, string name, DateTime birthDate, string phone)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(
                    @"UPDATE Patients 
                      SET Name = @Name, BirthDate = @BirthDate, Phone = @Phone
                      WHERE Id = @PatientId",
                    new { PatientId = patientId, Name = name, BirthDate = birthDate, Phone = phone }
                );

                return JsonSerializer.Serialize(new { success = true });
            }
        }

        public async Task<string> GetAvailableTimeSlots(int doctorId, DateTime date)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Отримуємо всі заняті слоти на вказану дату
                var busySlots = await connection.QueryAsync<DateTime>(
                    "SELECT DateTime FROM Appointments WHERE DoctorId = @DoctorId AND CAST(DateTime AS DATE) = @Date",
                    new { DoctorId = doctorId, Date = date.Date }
                );

                // Генеруємо всі можливі слоти для вказаної дати
                var workStart = date.Date.AddHours(9); // Початок робочого дня о 9:00
                var workEnd = date.Date.AddHours(18);  // Кінець робочого дня о 18:00
                var interval = TimeSpan.FromMinutes(30); // 30-хвилинні інтервали

                var allSlots = new List<object>();
                for (var slotStart = workStart; slotStart < workEnd; slotStart = slotStart.Add(interval))
                {
                    allSlots.Add(new
                    {
                        StartTime = slotStart,
                        IsAvailable = !busySlots.Contains(slotStart)
                    });
                }
                return JsonSerializer.Serialize(allSlots);
            }
        }
        public async Task<string> GetAllDoctorReviews()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var reviews = await connection.QueryAsync<dynamic>(
                    @"SELECT r.Id, r.Comment, r.Date, r.PatientId,
              d.Id as DoctorId, d.Name as DoctorName, d.Specialization,
              p.Name as PatientName
              FROM Reviews r
              JOIN Doctors d ON r.DoctorId = d.Id
              JOIN Patients p ON r.PatientId = p.Id
              ORDER BY r.Date DESC"
                );

                var formattedReviews = reviews.Select(r => new
                {
                    Id = r.Id,
                    Comment = r.Comment,
                    Date = r.Date,
                    Doctor = new
                    {
                        Id = r.DoctorId,
                        Name = r.DoctorName,
                        Specialization = r.Specialization
                    },
                    Patient = new
                    {
                        Id = r.PatientId,
                        Name = r.PatientName
                    }
                });

                return JsonSerializer.Serialize(formattedReviews);
            }
        }

        public async Task<string> GetPatientAppointments(int patientId)
{
    using (var connection = new SqlConnection(_connectionString))
    {
        await connection.OpenAsync();
        var appointments = await connection.QueryAsync<dynamic>(
            @"SELECT 
                a.Id,
                a.PatientId,
                a.DoctorId,
                a.DateTime,
                CASE 
                    WHEN a.DateTime > GETDATE() THEN 1 
                    ELSE 0 
                END as IsActive,
                d.Name as DoctorName,
                d.Specialization as DoctorSpecialization
            FROM Appointments a
            JOIN Doctors d ON a.DoctorId = d.Id
            WHERE a.PatientId = @PatientId
            ORDER BY a.DateTime DESC",
            new { PatientId = patientId }
        );

        var formattedAppointments = appointments.Select(a => new
        {
            Id = a.Id,
            PatientId = a.PatientId,
            DoctorId = a.DoctorId,
            DateTime = a.DateTime,
            IsActive = a.IsActive == 1,
            Doctor = new
            {
                Id = a.DoctorId,
                Name = a.DoctorName,
                Specialization = a.DoctorSpecialization
            }
        });

        return JsonSerializer.Serialize(formattedAppointments);
    }


}



    }
        }