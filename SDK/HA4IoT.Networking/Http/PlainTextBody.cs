using System;
using System.Text;
using HA4IoT.Contracts.Networking.Http;

namespace HA4IoT.Networking.Http
{
    public class PlainTextBody : IHttpBody
    {
        public StringBuilder Content { get; } = new StringBuilder();

        public string MimeType { get; set; } = MimeTypeProvider.PlainText;

        public byte[] ToByteArray()
        {
            return Encoding.UTF8.GetBytes(Content.ToString());
        }

        public PlainTextBody WithMimeType(string mimeType)
        {
            if (mimeType == null) throw new ArgumentNullException(nameof(mimeType));

            MimeType = mimeType;
            return this;
        }

        public PlainTextBody WithContent(string content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            Content.Append(content);
            return this;
        }
    }
}
