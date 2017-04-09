using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace HA4IoT.Networking.Http
{
    public class HttpResponseSerializer
    {
        public byte[] SerializeResponse(HttpContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var body = GetBody(context);

            context.Response.Headers[HttpHeaderName.ContentLength] = body.Length.ToString();

            if (body.Length == 0 && context.Response.StatusCode == HttpStatusCode.OK)
            {
                return GeneratePrefix(context.Response);
            }

            var prefix = GeneratePrefix(context.Response);
            var buffer = new byte[prefix.Length + body.Length];
            Array.Copy(prefix, 0, buffer, 0, prefix.Length);
            Array.Copy(body, 0, buffer, prefix.Length, body.Length);

            return buffer;
        }

        private byte[] GetBody(HttpContext context)
        {
            if (context.Response.StatusCode == HttpStatusCode.NotModified)
            {
                return new byte[0];
            }

            var content = new byte[0];
            if (context.Response.Body != null)
            {
                content = context.Response.Body;

                if (context.Response.MimeType != null)
                {
                    context.Response.Headers[HttpHeaderName.ContentType] = context.Response.MimeType;
                }

                if (context.Request.Headers.ClientSupportsGzipCompression())
                {
                    content = Compress(content);
                    context.Response.Headers[HttpHeaderName.ContentEncoding] = "gzip";
                }
            }

            return content;
        }

        private byte[] GeneratePrefix(HttpResponse response)
        {
            var buffer = new StringBuilder();
            buffer.AppendLine("HTTP/1.1 " + (int)response.StatusCode + " " + response.StatusCode);

            foreach (var header in response.Headers)
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
                using (var zipStream = new GZipStream(outputStream, CompressionLevel.Fastest))
                {
                    zipStream.Write(content, 0, content.Length);
                }

                return outputStream.ToArray();
            }
        }
    }
}
