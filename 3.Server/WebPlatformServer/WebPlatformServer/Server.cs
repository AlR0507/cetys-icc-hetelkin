using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WebPlatformServer
{
    public class Server
    {
        private readonly int _port;
        private readonly string _rootDirectory;
        private TcpListener _listener;

        public Server(int port = 8080, string rootDirectory = "static")
        {
            _port = port;
            _rootDirectory = rootDirectory;
        }

        public void Start()
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            Console.WriteLine($"Servidor iniciado en puerto {_port}, sirviendo directorio: {_rootDirectory}");

            while (true)
            {
                TcpClient client = _listener.AcceptTcpClient();
                Task.Run(() => HandleClient(client));
            }
        }

        private void HandleClient(TcpClient client)
        {
            using (client)
            using (NetworkStream stream = client.GetStream())
            using (StreamReader reader = new StreamReader(stream))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                try
                {
                    string requestLine = reader.ReadLine();
                    if (string.IsNullOrEmpty(requestLine))
                    {
                        SendErrorResponse(writer, "400 Bad Request");
                        return;
                    }

                    // Leer cabeceras hasta línea vacía
                    while (!string.IsNullOrEmpty(reader.ReadLine())) { }

                    string[] tokens = requestLine.Split(' ');
                    if (tokens.Length < 3)
                    {
                        SendErrorResponse(writer, "400 Bad Request");
                        return;
                    }

                    string method = tokens[0];
                    string url = tokens[1];

                    if (method != "GET")
                    {
                        SendErrorResponse(writer, "405 Method Not Allowed");
                        return;
                    }

                    if (url == "/") url = "/index.html";

                    string filePath = Path.Combine(_rootDirectory, url.TrimStart('/'));

                    if (!File.Exists(filePath))
                    {
                        SendErrorResponse(writer, "404 Not Found");
                        return;
                    }

                    byte[] content = File.ReadAllBytes(filePath);
                    string contentType = GetContentType(Path.GetExtension(filePath));

                    string response = $"HTTP/1.1 200 OK\r\n" +
                                      $"Content-Type: {contentType}\r\n" +
                                      $"Content-Length: {content.Length}\r\n" +
                                      $"\r\n";

                    writer.Write(Encoding.UTF8.GetBytes(response));
                    writer.Write(content);
                }
                catch
                {
                    SendErrorResponse(writer, "400 Bad Request");
                }
            }
        }

        private void SendErrorResponse(BinaryWriter writer, string status)
        {
            string body = $"<html><body><h1>{status}</h1></body></html>";
            string response = $"HTTP/1.1 {status}\r\n" +
                              "Content-Type: text/html\r\n" +
                              $"Content-Length: {Encoding.UTF8.GetByteCount(body)}\r\n" +
                              "\r\n" +
                              body;
            writer.Write(Encoding.UTF8.GetBytes(response));
        }

        private string GetContentType(string extension)
        {
            return extension.ToLower() switch
            {
                ".html" => "text/html",
                ".css" => "text/css",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".js" => "application/javascript",
                _ => "application/octet-stream"
            };
        }
    }
}