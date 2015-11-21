using System;
using System.Text;
using Windows.Data.Json;

namespace HA4IoT.Networking
{
    public class JsonBody : IHttpBody
    {
        public const string DefaultMimeType = "application/json; charset=utf-8";

        public JsonBody(JsonObject content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            Content = content;
        }

        public JsonObject Content { get; }

        public string MimeType { get; private set; } = DefaultMimeType;

        public byte[] ToByteArray()
        {
            return Encoding.UTF8.GetBytes(Content.Stringify());
        }

        public JsonBody WithMimeType(string mimeType)
        {
            if (mimeType == null) throw new ArgumentNullException(nameof(mimeType));

            MimeType = mimeType;
            return this;
        }
    }
}
