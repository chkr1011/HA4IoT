using System;
using System.Text;
using HA4IoT.Contracts.Networking.Http;

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

        public string MimeType { get; set; } = MimeTypeProvider.PlainText;

        public byte[] ToByteArray()
        {
            return Encoding.UTF8.GetBytes(Content);
        }
    }
}
