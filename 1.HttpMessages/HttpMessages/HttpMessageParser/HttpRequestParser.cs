using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HttpMessageParser.Models;

namespace HttpMessageParser
{
    public class HttpRequestParser : IRequestParser
    {
        public HttpRequest ParseRequest(string requestText)
        {
            //Validaciones iniciales
            if (requestText is null)
                throw new ArgumentNullException(nameof(requestText));

            if (string.IsNullOrWhiteSpace(requestText))
                throw new ArgumentException("Texto de solicitud vacio.", nameof(requestText));

            // Normalizo saltos de linea a '\n' 
            string normalized = requestText.Replace("\r\n", "\n");

            //Separo por lineas
            string[] lines = normalized.Split('\n');

            // Method SP RequestTarget SP Protocol
            string requestLine = lines[0].Trim();
            var parts = requestLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
                throw new ArgumentException("Formato de solicitud invalido.");

            string method = parts[0];
            string requestTarget = parts[1];
            string protocol = parts[2];

            // Reglas de validacion solicitadas
            // Metodo presente (ya garantizado por parts.Length==3)
            if (string.IsNullOrWhiteSpace(method))
                throw new ArgumentException("HTTP faltante.");

            //La ruta incluye al menos el caracter "/"
            if (string.IsNullOrWhiteSpace(requestTarget) || !requestTarget.Contains('/'))
                throw new ArgumentException("La solicitud debe incluir '/'.");

            //Protocolo presente e inicia con "HTTP"
            if (string.IsNullOrWhiteSpace(protocol) || !protocol.StartsWith("HTTP", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Protocolo debe empezar por 'HTTP'.");

            //Parseo de headers 
            var headers = new Dictionary<string, string>();
            int i = 1; 

            for (; i < lines.Length; i++)
            {
                var line = lines[i];

                // marca el fin de los headers
                if (line.Length == 0)
                {
                    i++; 
                    break;
                }

                // Cada header debe tener exactamente un ":" y texto antes ydespues
                int firstColon = line.IndexOf(':');
                if (firstColon <= 0) // implica que no hay ":" o esta en la posicion 0 
                    throw new ArgumentException($"Header invalido: '{line}'");

                // Debe haber exactamente un ":"
                if (line.IndexOf(':', firstColon + 1) != -1)
                    throw new ArgumentException($"El header debe contener al menos una ':': '{line}'");

                string name = line.Substring(0, firstColon).Trim();
                string value = line.Substring(firstColon + 1).Trim();

                if (name.Length == 0 || value.Length == 0)
                    throw new ArgumentException($"Nombre del header y el valor no debe ser vacio '{line}'");

                headers[name] = value;
            }

            //Body 
            string? body = null;
            if (i < lines.Length)
            {
                body = string.Join("\n", lines.Skip(i));
                if (string.IsNullOrEmpty(body))
                    body = null;
            }

            return new HttpRequest
            {
                Method = method,
                RequestTarget = requestTarget,
                Protocol = protocol,
                Headers = headers,
                Body = body
            };
        }
    }
}
