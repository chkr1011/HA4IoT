using System;

namespace HA4IoT.Contracts.Api
{
    public class ApiRequestReceivedEventArgs : EventArgs
    {
        public ApiRequestReceivedEventArgs(IApiCall apiCall)
        {
            if (apiCall == null) throw new ArgumentNullException(nameof(apiCall));

            ApiContext = apiCall;
        }

        public bool IsHandled { get; set; }

        public IApiCall ApiContext { get; }
    }
}
