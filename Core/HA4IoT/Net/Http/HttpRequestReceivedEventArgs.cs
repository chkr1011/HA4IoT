using System;

namespace HA4IoT.Net.Http
{
    public class HttpRequestReceivedEventArgs : EventArgs
    {
        public HttpRequestReceivedEventArgs(HttpContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public HttpContext Context { get; }

        public bool IsHandled { get; set; }
    }
}
