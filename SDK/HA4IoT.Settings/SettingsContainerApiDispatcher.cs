using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Settings
{
    public class SettingsContainerApiDispatcher
    {
        private readonly ISettingsContainer _settingsContainer;
        private readonly IApiService _apiController;
        private readonly string _relativeUri;

        public SettingsContainerApiDispatcher(ISettingsContainer settingsContainer, string relativeUri, IApiService apiController)
        {
            if (settingsContainer == null) throw new ArgumentNullException(nameof(settingsContainer));
            if (relativeUri == null) throw new ArgumentNullException(nameof(relativeUri));
            if (apiController == null) throw new ArgumentNullException(nameof(apiController));

            _settingsContainer = settingsContainer;
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
            apiContext.Response = _settingsContainer.Export();
        }

        private void HandleApiCommand(IApiContext apiContext)
        {
            _settingsContainer.Import(apiContext.Request);
            _settingsContainer.Save();
        }
    }
}
