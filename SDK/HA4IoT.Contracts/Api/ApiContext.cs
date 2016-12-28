using System;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Api
{
    public class ApiContext : IApiContext
    {
        public ApiContext(string action, JObject request, JObject response)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (response == null) throw new ArgumentNullException(nameof(response));

            ResultCode = ApiResultCode.Success;

            Action = action;
            Parameter = request;
            Response = response;
        }

        public string Action { get; }
        public ApiResultCode ResultCode { get; set; }
        public JObject Parameter { get; }
        public JObject Response { get; set; }
        public bool UseHash { get; set; }
    }
}
