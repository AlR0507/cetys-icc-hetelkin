using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HttpMessageParser.Models;

namespace HttpMessageParser
{
    public class HttpResponseWriter : IResponseWriter
    {
        public string WriteResponse(HttpResponse response)
        {
            //Validaciones
            if (response is null)
                throw new ArgumentNullException(nameof(response));

            if (string.IsNullOrWhiteSpace(response.Protocol))
                throw new ArgumentException("Se requiere Response.Protocol");

            if (string.IsNullOrWhiteSpace(response.StatusText))
                throw new ArgumentException("Se requiere Response.StatusText");

            if (response.Headers is null)
                throw new ArgumentException("Response.Headers no debe ser null.");

            // HTTP/x.y StatusCode StatusText
            var sb = new StringBuilder();
            sb.Append($"{response.Protocol} {response.StatusCode} {response.StatusText}");

            //Headers, cada uno en su propia linea
            foreach (var kvp in response.Headers)
            {
                sb.Append($"\n{kvp.Key}: {kvp.Value}");
            }

            //Body
            if (response.Body is not null)
            {
                sb.Append($"\n{response.Body}");
            }

            return sb.ToString();
        }
    }
}

