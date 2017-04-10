using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Windows.Web.Http;

namespace HA4IoT.Networking.Http
{
    public class HttpResponseSerializer
    {
        public byte[] SerializeResponse(HttpContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var body = GenerateBody(context);

            context.Response.Headers[HttpHeaderName.ContentLength] = body.Length.ToString();

            var response = GenerateResponseWithHeaders(context.Response);

            if (body.Length == 0)
            {
                return response;
            }

            var buffer = new byte[response.Length + body.Length];
            Array.Copy(response, 0, buffer, 0, response.Length);
            Array.Copy(body, 0, buffer, response.Length, body.Length);

            return buffer;
        }

        private static byte[] GenerateResponseWithHeaders(HttpResponse response)
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

        private static byte[] GenerateBody(HttpContext context)
        {
            if (context.Response.StatusCode == HttpStatusCode.NotModified)
            {
                return new byte[0];
            }

            if (context.Response.Body == null)
            {
                return new byte[0];
            }

            if (context.Response.MimeType != null)
            {
                context.Response.Headers[HttpHeaderName.ContentType] = context.Response.MimeType;
            }

            var content = context.Response.Body;
            if (context.Request.Headers.ClientSupportsGzipCompression())
            {
                content = Compress(content);
                context.Response.Headers[HttpHeaderName.ContentEncoding] = "gzip";
            }

            return content;
        }

        private static byte[] Compress(byte[] content)
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
