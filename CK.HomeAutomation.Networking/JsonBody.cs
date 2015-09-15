using System;
using System.Text;
using Windows.Data.Json;

namespace CK.HomeAutomation.Networking
{
    public class JsonBody : IBody
    {
        public JsonBody(JsonObject content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            Content = content;
        }

        public JsonObject Content { get; }

        public string MimeType { get; private set; } = "application/json";

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
