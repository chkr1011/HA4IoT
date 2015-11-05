using System;
using System.Collections.Generic;

namespace HA4IoT.Networking
{
    public class BinaryBody : IBody
    {
        public BinaryBody(IEnumerable<byte> content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            Content.AddRange(content);
        }

        public List<byte> Content { get; } = new List<byte>();

        public string MimeType { get; private set; } = "application/octet-stream";

        public byte[] ToByteArray()
        {
            return Content.ToArray();
        }

        public BinaryBody WithMimeType(string mimeType)
        {
            if (mimeType == null) throw new ArgumentNullException(nameof(mimeType));

            MimeType = mimeType;
            return this;
        }
    }
}
