using System;
using Windows.Data.Json;
using HA4IoT.Networking;

namespace HA4IoT.Automations
{
    public class AutomationSettingsHttpApiDispatcher
    {
        private readonly AutomationSettings _automationSettings;
        private readonly IHttpRequestController _httpApiController;

        public AutomationSettingsHttpApiDispatcher(AutomationSettings automationSettingsContainer, IHttpRequestController httpApiController)
        {
            if (automationSettingsContainer == null) throw new ArgumentNullException(nameof(automationSettingsContainer));
            if (httpApiController == null) throw new ArgumentNullException(nameof(httpApiController));

            _automationSettings = automationSettingsContainer;
            _httpApiController = httpApiController;
        }

        public void ExposeToApi()
        {
            _httpApiController.Handle(HttpMethod.Post, $"automation/{_automationSettings.AutomationId}/settings").Using(HandleApiPost);
            _httpApiController.Handle(HttpMethod.Get, $"automation/{_automationSettings.AutomationId}/settings").Using(HandleApiGet);
        }

        private void HandleApiGet(HttpContext httpContext)
        {
            httpContext.Response.Body = new JsonBody(_automationSettings.ExportToJsonObject());
        }

        private void HandleApiPost(HttpContext httpContext)
        {
            JsonObject requestBody;
            if (!JsonObject.TryParse(httpContext.Request.Body, out requestBody))
            {
                return;
            }

            _automationSettings.ImportFromJsonObject(requestBody);
        }
    }
}
