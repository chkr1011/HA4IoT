using System;
using System.Collections.Generic;

namespace HA4IoT.Net.Http
{
    public static class HttpHeaderExtensions
    {
        public static bool ConnectionMustBeClosed(this Dictionary<string, string> headers)
        {
            string value;
            return headers.TryGetValue(HttpHeaderName.Connection, out value) &&
                string.Equals(value, "Close", StringComparison.OrdinalIgnoreCase);
        }

        public static bool ClientSupportsGzipCompression(this Dictionary<string, string> headers)
        {
            string headerValue;
            if (headers.TryGetValue(HttpHeaderName.AcceptEncoding, out headerValue))
            {
                return headerValue.IndexOf("gzip", StringComparison.OrdinalIgnoreCase) > -1;
            }

            return false;
        }

        public static bool RequiresContinue(this Dictionary<string, string> headers)
        {
            string value;
            return headers.TryGetValue(HttpHeaderName.Expect, out value) &&
                string.Equals(value, "100-Continue", StringComparison.OrdinalIgnoreCase);
        }

        public static bool ValueEquals(this Dictionary<string, string> headers, string headerName, string expectedValue)
        {
            if (headers == null) throw new ArgumentNullException(nameof(headers));
            if (headerName == null) throw new ArgumentNullException(nameof(headerName));
            if (expectedValue == null) throw new ArgumentNullException(nameof(expectedValue));

            string value;
            if (!headers.TryGetValue(headerName, out value))
            {
                return false;
            }

            return string.Equals(value, expectedValue, StringComparison.OrdinalIgnoreCase);
        }
    }
}
