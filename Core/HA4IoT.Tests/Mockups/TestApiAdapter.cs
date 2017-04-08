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

        public IApiContext Invoke(string action, JObject parameter)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (parameter == null) throw new ArgumentNullException(nameof(parameter));

            var apiContext = new ApiContext(action, parameter, null);
            RequestReceived?.Invoke(this, new ApiRequestReceivedEventArgs(apiContext));

            return apiContext;
        }
    }
}
