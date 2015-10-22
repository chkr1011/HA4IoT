using System;
using Windows.Data.Json;

namespace HA4IoT.Actuators
{
    public class ApiRequestContext
    {
        public ApiRequestContext(JsonObject request, JsonObject response)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (response == null) throw new ArgumentNullException(nameof(response));

            Request = request;
            Response = response;
        }

        public JsonObject Request { get; }

        public JsonObject Response { get; }
    }
}
