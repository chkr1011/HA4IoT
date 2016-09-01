using System.Text;

namespace HA4IoT.Networking
{
    public static class ByteExtensions
    {
        public static byte[] ToByteArray(this string data)
        {
            if (data == null)
            {
                return null;
            }

            if (data.Length == 0)
            {
                return new byte[0];
            }

            return Encoding.UTF8.GetBytes(data);
        }

        public static byte[] ToByteArray(this byte data)
        {
            return new[] { data};
        }
    }
}
