using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using HA4IoT.Contracts.Networking.Http;

namespace HA4IoT.Networking.Http
{
    public class HttpResponseSerializer
    {
        private readonly StatusDescriptionProvider _statusDescriptionProvider = new StatusDescriptionProvider();

        public byte[] SerializeResponse(HttpContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var body = GetBody(context);

            if (body.Length == 0 && context.Response.StatusCode == HttpStatusCode.OK)
            {
                context.Response.StatusCode = HttpStatusCode.NoContent;
                return GeneratePrefix(context.Response);
            }

            context.Response.Headers[HttpHeaderNames.ContentLength] = body.Length.ToString();

            var prefix = GeneratePrefix(context.Response);
            var buffer = new byte[prefix.Length + body.Length];
            Array.Copy(prefix, 0, buffer, 0, prefix.Length);
            Array.Copy(body, 0, buffer, prefix.Length, body.Length);

            return buffer;

            // TODO: Delete if above code is working.
            //var prefix = GeneratePrefix(context.Response);
            ////using (var buffer = new MemoryStream(prefix.Length + body.Length))
            ////{
            ////    buffer.Write(prefix, 0, prefix.Length);

            ////    if (body.Length > 0)
            ////    {
            ////        buffer.Write(body, 0, body.Length);
            ////    }

            ////    return buffer.ToArray();
            ////}
        }

        private byte[] GetBody(HttpContext context)
        {
            if (context.Response.StatusCode == HttpStatusCode.NotModified)
            {
                return new byte[0];
            }

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

            return content;
        }

        private byte[] GeneratePrefix(HttpResponse response)
        {
            var statusDescription = _statusDescriptionProvider.GetDescription(response.StatusCode);

            var buffer = new StringBuilder();
            buffer.AppendLine("HTTP/1.1 " + (int)response.StatusCode + " " + statusDescription);

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
                using (var zipStream = new GZipStream(outputStream, CompressionLevel.Optimal))
                {
                    zipStream.Write(content, 0, content.Length);
                }

                return outputStream.ToArray();
            }
        }
    }
}
