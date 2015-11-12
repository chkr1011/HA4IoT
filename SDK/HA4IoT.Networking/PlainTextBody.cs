using System;
using System.Text;

namespace HA4IoT.Networking
{
    public class PlainTextBody : IHttpBody
    {
        public const string DefaultMimeType = "text/plain; charset=utf-8";

        public StringBuilder Content { get; } = new StringBuilder();

        public string MimeType { get; set; } = DefaultMimeType;

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
