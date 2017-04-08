namespace HA4IoT.Networking.Http
{
    public interface IHttpBody
    {
        string MimeType { get; }

        byte[] ToByteArray();
    }
}
