using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Core.Settings
{
    public class SettingsContainerApiDispatcher<TSettings> where TSettings : ISettingsContainer
    {
        private readonly TSettings _settingsContainer;
        private readonly IApiController _apiController;
        private readonly string _relativeUri;

        public SettingsContainerApiDispatcher(TSettings settingsContainerContainer, string relativeUri, IApiController apiController)
        {
            if (settingsContainerContainer == null) throw new ArgumentNullException(nameof(settingsContainerContainer));
            if (relativeUri == null) throw new ArgumentNullException(nameof(relativeUri));
            if (apiController == null) throw new ArgumentNullException(nameof(apiController));

            _settingsContainer = settingsContainerContainer;
            _apiController = apiController;

            _relativeUri = relativeUri;
        }

        public void ExposeToApi()
        {
            _apiController.RouteCommand($"{_relativeUri}/settings", HandleApiCommand);
            _apiController.RouteRequest($"{_relativeUri}/settings", HandleApiRequest);
        }

        private void HandleApiRequest(IApiContext apiContext)
        {
            apiContext.Response = _settingsContainer.ExportToJsonObject();
        }

        private void HandleApiCommand(IApiContext apiContext)
        {
            _settingsContainer.ImportFromJsonObject(apiContext.Request);
        }
    }
}
