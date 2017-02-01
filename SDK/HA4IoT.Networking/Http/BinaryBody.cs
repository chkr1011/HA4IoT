namespace HA4IoT.Networking.Http
{
    public class BinaryBody : IHttpBody
    {
        public byte[] Content { get; set; }

        public string MimeType { get; set; } = MimeTypeProvider.OctetStream;

        public byte[] ToByteArray()
        {
            return Content;
        }
    }
}
