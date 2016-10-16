using System;
using HA4IoT.Contracts.Networking.Http;

namespace HA4IoT.Networking.Http
{
    public static class HttpRequestExtensions
    {
        public static bool GetRequiresContinue(this HttpRequest httpRequest)
        {
            if (httpRequest == null) throw new ArgumentNullException(nameof(httpRequest));

            if (!httpRequest.Headers.ContainsKey(HttpHeaderNames.ContentLength))
            {
                return false;
            }
            
            if (httpRequest.Headers[HttpHeaderNames.ContentLength] != httpRequest.BinaryBodyLength.ToString())
            {
                return true;
            }

            return httpRequest.Headers.GetRequiresContinue();
        }
    }
}
