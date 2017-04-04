using System.Text;

namespace HA4IoT.Networking.Http
{
    public class PlainTextBody : IHttpBody
    {
        public string MimeType { get; set; } = MimeTypeProvider.PlainText;

        public string Content { get; set; }

        public byte[] ToByteArray()
        {
            return Encoding.UTF8.GetBytes(Content ?? string.Empty);
        }
    }
}
