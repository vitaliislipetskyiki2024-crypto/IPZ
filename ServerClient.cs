using System;
using System.Net.Sockets;
using System.Text;

namespace CarServiceApp
{
    public static class ServerClient
    {
        private const string Host = "127.0.0.1";
        private const int Port = 8888;

        public static string SendRequest(string requestData)
        {
            try
            {
                using (TcpClient client = new TcpClient(Host, Port))
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] data = Encoding.UTF8.GetBytes(requestData);
                    stream.Write(data, 0, data.Length);

                    byte[] buffer = new byte[2048];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    return Encoding.UTF8.GetString(buffer, 0, bytesRead);
                }
            }
            catch (Exception ex)
            {
                return $"ERR#Помилка підключення до сервера: {ex.Message}";
            }
        }
    }
}