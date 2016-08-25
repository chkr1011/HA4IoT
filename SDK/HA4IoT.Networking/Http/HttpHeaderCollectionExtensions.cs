using System;
using HA4IoT.Contracts.Networking;
using HA4IoT.Contracts.Networking.Http;

namespace HA4IoT.Networking.Http
{
    public static class HttpHeaderCollectionExtensions
    {
        public static bool GetConnectionMustBeClosed(this HttpHeaderCollection headers)
        {
            string value;
            return headers.TryGetValue(HttpHeaderNames.Connection, out value) &&
                string.Equals(value, "Close", StringComparison.OrdinalIgnoreCase);
        }

        public static bool GetClientSupportsGzipCompression(this HttpHeaderCollection headers)
        {
            string headerValue;
            if (headers.TryGetValue(HttpHeaderNames.AcceptEncoding, out headerValue))
            {
                return headerValue.IndexOf("gzip", StringComparison.OrdinalIgnoreCase) > -1;
            }

            return false;
        }

        public static bool GetRequiresContinue(this HttpHeaderCollection headers)
        {
            string value;
            return headers.TryGetValue(HttpHeaderNames.Expect, out value) &&
                string.Equals(value, "100-Continue", StringComparison.OrdinalIgnoreCase);
        }

        public static bool GetHasBodyContent(this HttpHeaderCollection headers)
        {
            string value;
            return headers.TryGetValue(HttpHeaderNames.ContentLength, out value) &&
                !string.Equals(value, "0", StringComparison.OrdinalIgnoreCase);
        }
    }
}
