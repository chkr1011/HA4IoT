namespace CK.HomeAutomation.Networking
{
    public interface IBody
    {
        string MimeType { get; }

        byte[] ToByteArray();
    }
}
