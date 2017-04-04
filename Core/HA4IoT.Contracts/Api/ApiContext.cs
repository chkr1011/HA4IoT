using System;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Api
{
    public class ApiContext : IApiContext
    {
        public ApiContext(string action, JObject parameter, string resultHash)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (parameter == null) throw new ArgumentNullException(nameof(parameter));

            ResultCode = ApiResultCode.Success;

            Action = action;
            Parameter = parameter;
            ResultHash = resultHash;
        }

        public string Action { get; }
        public JObject Parameter { get; }

        public ApiResultCode ResultCode { get; set; }
        public JObject Result { get; set; } = new JObject();
        public string ResultHash { get; set; }
    }
}
