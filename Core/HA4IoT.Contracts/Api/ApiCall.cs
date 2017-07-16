using Newtonsoft.Json.Linq;
using System;

namespace HA4IoT.Contracts.Api
{
    public class ApiCall : IApiCall
    {
        public ApiCall(string action, JObject parameter, string resultHash)
        {
            ResultCode = ApiResultCode.Success;

            Action = action ?? throw new ArgumentNullException(nameof(action));
            Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
            ResultHash = resultHash;
        }

        public string Action { get; }
        public JObject Parameter { get; }

        public ApiResultCode ResultCode { get; set; }
        public JObject Result { get; set; } = new JObject();
        public string ResultHash { get; set; }
    }
}
