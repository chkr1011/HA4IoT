using System;

namespace HA4IoT.Contracts.Api
{
    public class ApiRequestReceivedEventArgs : EventArgs
    {
        public ApiRequestReceivedEventArgs(IApiContext apiContext)
        {
            if (apiContext == null) throw new ArgumentNullException(nameof(apiContext));

            ApiContext = apiContext;
        }

        public bool IsHandled { get; set; }

        public IApiContext ApiContext { get; }
    }
}
