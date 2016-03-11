using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Core.Settings
{
    public class SettingsContainerHttpApiDispatcher<TSettings> where TSettings : ISettingsContainer
    {
        private readonly TSettings _settingsContainer;
        private readonly IApiController _apiController;
        private readonly string _relativeUri;

        public SettingsContainerHttpApiDispatcher(TSettings settingsContainerContainer, string relativeUri, IApiController apiController)
        {
            if (settingsContainerContainer == null) throw new ArgumentNullException(nameof(settingsContainerContainer));
            if (apiController == null) throw new ArgumentNullException(nameof(apiController));

            _settingsContainer = settingsContainerContainer;
            _apiController = apiController;

            _relativeUri = relativeUri;
        }

        public void ExposeToApi()
        {
            _apiController.RouteCommand($"{_relativeUri}/settings", HandleApiPost);
            _apiController.RouteRequest($"{_relativeUri}/settings", HandleApiGet);
        }

        private void HandleApiGet(IApiContext apiContext)
        {
            apiContext.Response = _settingsContainer.ExportToJsonObject();
        }

        private void HandleApiPost(IApiContext apiContext)
        {
            _settingsContainer.ImportFromJsonObject(apiContext.Request);
        }
    }
}
