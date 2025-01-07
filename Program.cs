using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using HosregServer1;

namespace HosregServer1
{
    class Program
    {
        private static TcpListener? _server;
        private const int Port = 8888;
        private static readonly DatabaseService _database = new DatabaseService();

        static async Task Main(string[] args)
        {
            try
            {
                // Перевіряємо підключення до бази даних перед запуском сервера
                Console.WriteLine("Перевірка підключення до бази даних...");
                if (!await _database.TestConnection())
                {
                    Console.WriteLine("Сервер не може запуститися через помилку підключення до бази даних.");
                    Console.WriteLine("Натисніть будь-яку клавішу для виходу...");
                    Console.ReadKey();
                    return;
                }

                _server = new TcpListener(IPAddress.Any, Port);
                _server.Start();
                Console.WriteLine($"Сервер успішно запущено на порту {Port}");

                while (true)
                {
                    Console.WriteLine("Очікування підключення клієнта...");
                    TcpClient client = await _server.AcceptTcpClientAsync();
                    _ = HandleClientAsync(client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка сервера: {ex.Message}");
            }
        }

        private static async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                using NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[4096];

                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string jsonRequest = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Отримано запит: {jsonRequest}");

                    string jsonResponse = await ProcessRequest(jsonRequest);
                    byte[] responseData = Encoding.UTF8.GetBytes(jsonResponse);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка обробки клієнта: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        private static async Task<string> ProcessRequest(string jsonRequest)
        {
            try
            {
                var request = JsonSerializer.Deserialize<JsonElement>(jsonRequest);
                string command = request.GetProperty("command").GetString();

                switch (command)
                {
                    case "LOGIN":
                        string login = request.GetProperty("login").GetString();
                        string password = request.GetProperty("password").GetString();
                        return await _database.AuthenticateUser(login, password);

                    case "GET_PATIENTS":
                        return await _database.GetAllPatients();

                    case "GET_PATIENT_DATA":
                        int patientId = request.GetProperty("patientId").GetInt32();
                        return await _database.GetPatientData(patientId);

                    case "GET_DOCTORS":
                        return await _database.GetAllDoctors();

                    case "GET_APPOINTMENTS":
                        patientId = request.GetProperty("patientId").GetInt32();
                        return await _database.GetPatientAppointments(patientId);

                    case "GET_REVIEWS":
                        patientId = request.GetProperty("patientId").GetInt32();
                        return await _database.GetPatientReviews(patientId);

                    

                    case "CREATE_REVIEW":
                        return await _database.CreateReview(
                            request.GetProperty("patientId").GetInt32(),
                            request.GetProperty("doctorId").GetInt32(),
                            request.GetProperty("comment").GetString(),
                            request.GetProperty("date").GetDateTime()
                        );

                    case "UPDATE_PATIENT":
                        return await _database.UpdatePatient(
                            request.GetProperty("patientId").GetInt32(),
                            request.GetProperty("name").GetString(),
                            request.GetProperty("birthDate").GetDateTime(),
                            request.GetProperty("phone").GetString()
                        );

                    case "GET_AVAILABLE_TIMESLOTS":
                        return await _database.GetAvailableTimeSlots(
                            request.GetProperty("doctorId").GetInt32(),
                            request.GetProperty("date").GetDateTime()
                        );
                    case "GET_ALL_DOCTOR_REVIEWS":
                        return await _database.GetAllDoctorReviews();

                    case "CREATE_APPOINTMENT":
                        return await _database.CreateAppointment(
                            request.GetProperty("patientId").GetInt32(),
                            request.GetProperty("doctorId").GetInt32(),
                            request.GetProperty("dateTime").GetDateTime()
                        );



                    default:
                        return JsonSerializer.Serialize(new { error = "Unknown command" });
                }
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }
    }
}