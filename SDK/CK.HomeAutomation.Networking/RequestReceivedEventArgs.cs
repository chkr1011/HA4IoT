using System;

namespace CK.HomeAutomation.Networking
{
    public class RequestReceivedEventArgs : EventArgs
    {
        public RequestReceivedEventArgs(HttpContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            Context = context;
        }

        public HttpContext Context { get; }

        public bool IsHandled { get; set; }
    }
}
