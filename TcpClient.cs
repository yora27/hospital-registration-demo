using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hosreg1
{
    public class TcpClientService
    {
        private TcpClient? _client;
        private NetworkStream? _stream;
        private readonly string _serverIp = "127.0.0.1";
        private readonly int _port = 8888;
        private readonly JsonSerializerOptions _jsonOptions;

        public TcpClientService()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task ConnectAsync()
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(_serverIp, _port);
                _stream = _client.GetStream();
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка підключення до сервера: {ex.Message}");
            }
        }

        public async Task<T> SendRequestAsync<T>(object request)
        {
            if (_client == null || !_client.Connected)
                throw new Exception("Клієнт не підключений до сервера");

            try
            {
                string jsonRequest = JsonSerializer.Serialize(request);
                byte[] data = Encoding.UTF8.GetBytes(jsonRequest);
                await _stream!.WriteAsync(data, 0, data.Length);

                byte[] buffer = new byte[4096];
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                string jsonResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                return JsonSerializer.Deserialize<T>(jsonResponse, _jsonOptions);

            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка відправки запиту: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            _stream?.Close();
            _client?.Close();
        }
    }
}