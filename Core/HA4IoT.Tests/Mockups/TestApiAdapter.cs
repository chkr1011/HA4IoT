using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Tests.Mockups
{
    public class TestApiAdapter : IApiAdapter
    {
        public event EventHandler<ApiRequestReceivedEventArgs> RequestReceived;

        public int NotifyStateChangedCalledCount { get; set; }

        public void NotifyStateChanged(IComponent component)
        {
            NotifyStateChangedCalledCount++;
        }

        public IApiCall Invoke(string action, JObject parameter)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (parameter == null) throw new ArgumentNullException(nameof(parameter));

            var apiCall = new ApiCall(action, parameter, null);
            RequestReceived?.Invoke(this, new ApiRequestReceivedEventArgs(apiCall));

            return apiCall;
        }
    }
}
