using System;
namespace HA4IoT.Networking
{
    public static class HttpHeaderCollectionExtensions
    {
        public static bool GetConnectedCanStayOpen(this HttpHeaderCollection headers)
        {
            string value;
            return headers.TryGetValue(HttpHeaderNames.Connection, out value) &&
                string.Equals(value, "Keep-Alive", StringComparison.OrdinalIgnoreCase);
        }

        public static bool GetClientSupportsGzipCompression(this HttpHeaderCollection headers)
        {
            string headerValue;
            if (headers.TryGetValue("Accept-Encoding", out headerValue))
            {
                return headerValue.IndexOf("gzip", StringComparison.OrdinalIgnoreCase) > -1;
            }

            return false;
        }
    }
}
