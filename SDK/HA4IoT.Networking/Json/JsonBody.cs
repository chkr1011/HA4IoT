using System;
using System.Text;
using HA4IoT.Contracts.Networking.Http;
using HA4IoT.Networking.Http;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Networking.Json
{
    public class JsonBody : IHttpBody
    {
        public JsonBody(JToken content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            
            Content = content;            
        }

        public JToken Content { get; }

        public string MimeType { get; set; } = MimeTypeProvider.Json;

        public byte[] ToByteArray()
        {
            return Encoding.UTF8.GetBytes(Content.ToString());
        }
    }
}
