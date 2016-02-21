namespace HA4IoT.Contracts.Networking
{
    public interface IHttpBody
    {
        string MimeType { get; }

        byte[] ToByteArray();
    }
}
