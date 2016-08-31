using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Networking.Http;

namespace HA4IoT.Networking.Http
{
    public class BinaryBody : IHttpBody
    {
        public BinaryBody(IEnumerable<byte> content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            Content.AddRange(content);
        }

        public List<byte> Content { get; } = new List<byte>();

        public string MimeType { get; set; } = MimeTypeProvider.OctetStream;

        public byte[] ToByteArray()
        {
            return Content.ToArray();
        }

        public BinaryBody WithContent(IEnumerable<byte> content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            Content.AddRange(content);
            return this;
        }

        public BinaryBody WithMimeType(string mimeType)
        {
            if (mimeType == null) throw new ArgumentNullException(nameof(mimeType));

            MimeType = mimeType;
            return this;
        }
    }
}
