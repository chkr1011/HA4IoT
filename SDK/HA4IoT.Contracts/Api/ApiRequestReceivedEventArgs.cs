using System;

namespace HA4IoT.Contracts.Api
{
    public class ApiRequestReceivedEventArgs : EventArgs
    {
        public ApiRequestReceivedEventArgs(IApiContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            Context = context;
        }

        public bool IsHandled { get; set; }

        public IApiContext Context { get; }
    }
}
