using System;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Api
{
    public class ApiContext : IApiContext
    {
        public ApiContext(string uri, JObject request, JObject response)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (response == null) throw new ArgumentNullException(nameof(response));

            ResultCode = ApiResultCode.Success;

            Uri = uri;
            Request = request;
            Response = response;
        }

        public string Uri { get; }
        public ApiResultCode ResultCode { get; set; }
        public JObject Request { get; }
        public JObject Response { get; set; }
        public bool UseHash { get; set; }
    }
}
