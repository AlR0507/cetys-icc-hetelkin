using System;

namespace WebPlatformServer.ConsoleRunner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Configuración básica
            int port = 8080;
            string directory = "static";

            // Procesar argumentos simples
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--port" && i + 1 < args.Length)
                    int.TryParse(args[i + 1], out port);
                if (args[i] == "--directory" && i + 1 < args.Length)
                    directory = args[i + 1];
            }

            try
            {
                Server server = new Server(port, directory);
                server.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}