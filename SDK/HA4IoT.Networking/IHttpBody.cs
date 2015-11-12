namespace HA4IoT.Networking
{
    public interface IHttpBody
    {
        string MimeType { get; }

        byte[] ToByteArray();
    }
}
