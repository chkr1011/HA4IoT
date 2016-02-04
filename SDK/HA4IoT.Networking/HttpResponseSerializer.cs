using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace HA4IoT.Networking
{
    public class HttpResponseSerializer
    {
        private readonly StatusDescriptionProvider _statusDescriptionProvider = new StatusDescriptionProvider();

        public byte[] SerializeResponse(HttpContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            
            byte[] content = new byte[0];
            if (context.Response.Body != null)
            {
                content = context.Response.Body.ToByteArray();
                context.Response.Headers[HttpHeaderNames.ContentType] = context.Response.Body.MimeType;

                if (context.Request.Headers.GetClientSupportsGzipCompression())
                {
                    content = Compress(content);
                    context.Response.Headers[HttpHeaderNames.ContentEncoding] = "gzip";
                }
            }

            context.Response.Headers[HttpHeaderNames.ContentLength] = content.Length.ToString();

            byte[] prefix = GeneratePrefix(context.Response);

            using (var buffer = new MemoryStream())
            {
                buffer.Write(prefix, 0, prefix.Length);
                buffer.Write(content, 0, content.Length);

                return buffer.ToArray();
            }
        }

        private byte[] GeneratePrefix(HttpResponse response)
        {
            var statusDescription = _statusDescriptionProvider.GetDescription(response.StatusCode);

            var buffer = new StringBuilder();
            buffer.AppendLine("HTTP/1.1 " + (int)response.StatusCode + " " + statusDescription);

            foreach (KeyValuePair<string, string> header in response.Headers)
            {
                buffer.AppendLine(header.Key + ":" + header.Value);
            }

            buffer.AppendLine();

            return Encoding.UTF8.GetBytes(buffer.ToString());
        }

        private byte[] Compress(byte[] content)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var zipStream = new GZipStream(outputStream, CompressionLevel.Optimal))
                {
                    zipStream.Write(content, 0, content.Length);
                }

                return outputStream.ToArray();
            }
        }
    }
}
