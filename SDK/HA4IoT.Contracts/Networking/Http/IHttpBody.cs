namespace HA4IoT.Contracts.Networking.Http
{
    public interface IHttpBody
    {
        string MimeType { get; }

        byte[] ToByteArray();
    }
}
