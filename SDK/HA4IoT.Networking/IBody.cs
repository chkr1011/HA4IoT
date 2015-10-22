namespace HA4IoT.Networking
{
    public interface IBody
    {
        string MimeType { get; }

        byte[] ToByteArray();
    }
}
