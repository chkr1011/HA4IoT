using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Contracts.Networking;
using HA4IoT.Networking;

namespace HA4IoT.Core.Settings
{
    public class SettingsContainerHttpApiDispatcher<TSettings> where TSettings : ISettingsContainer
    {
        private readonly TSettings _settingsContainer;
        private readonly IHttpRequestController _httpApiController;
        private readonly string _relativeUri;

        public SettingsContainerHttpApiDispatcher(TSettings settingsContainerContainer, string relativeUri, IHttpRequestController httpApiController)
        {
            if (settingsContainerContainer == null) throw new ArgumentNullException(nameof(settingsContainerContainer));
            if (httpApiController == null) throw new ArgumentNullException(nameof(httpApiController));

            _settingsContainer = settingsContainerContainer;
            _httpApiController = httpApiController;

            _relativeUri = relativeUri;
        }

        public void ExposeToApi()
        {
            _httpApiController.HandlePost($"{_relativeUri}/settings").Using(HandleApiPost);
            _httpApiController.HandleGet($"{_relativeUri}/settings").Using(HandleApiGet);
        }

        private void HandleApiGet(HttpContext httpContext)
        {
            httpContext.Response.Body = new JsonBody(_settingsContainer.ExportToJsonObject());
        }

        private void HandleApiPost(HttpContext httpContext)
        {
            JsonObject requestBody;
            if (!JsonObject.TryParse(httpContext.Request.Body, out requestBody))
            {
                return;
            }

            _settingsContainer.ImportFromJsonObject(requestBody);
        }
    }
}
