using System;
using System.Text;

namespace CK.HomeAutomation.Networking
{
    public class PlainTextBody : IBody
    {
        public StringBuilder Content { get; } = new StringBuilder();

        public string MimeType { get; set; } = "text/plain; charset=utf-8";

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
    }
}
