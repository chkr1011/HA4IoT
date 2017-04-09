using System;

namespace HA4IoT.Networking.Http
{
    public class BinaryBody : IHttpBody
    {
        public BinaryBody()
        {
        }

        public BinaryBody(byte[] content, string mimeType)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            MimeType = mimeType ?? throw new ArgumentNullException(nameof(mimeType));
        }

        public byte[] Content { get; set; }

        public string MimeType { get; set; } = MimeTypeProvider.OctetStream;

        public byte[] ToByteArray()
        {
            return Content;
        }
    }
}
