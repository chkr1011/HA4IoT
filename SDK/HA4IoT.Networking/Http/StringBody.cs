using System;
using System.Text;
using HA4IoT.Contracts.Networking;

namespace HA4IoT.Networking.Http
{
    public class StringBody : IHttpBody
    {
        public StringBody(string content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            
            Content = content;            
        }

        public string Content { get; }

        public string MimeType { get; private set; } = MimeTypeProvider.PlainText;

        public byte[] ToByteArray()
        {
            return Encoding.UTF8.GetBytes(Content);
        }

        public StringBody WithMimeType(string mimeType)
        {
            if (mimeType == null) throw new ArgumentNullException(nameof(mimeType));

            MimeType = mimeType;
            return this;
        }
    }
}
