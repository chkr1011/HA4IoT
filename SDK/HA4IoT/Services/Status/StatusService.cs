using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Settings;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Services.Status
{
    [ApiServiceClass(typeof(StatusService))] // TODO: Use IStatusService
    public class StatusService : ServiceBase
    {
        private readonly IComponentService _componentService;
        private readonly ISettingsService _settingsService;

        public StatusService(IComponentService componentService, IApiDispatcherService apiService, ISettingsService settingsService)
        {
            if (componentService == null) throw new ArgumentNullException(nameof(componentService));
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            _componentService = componentService;
            _settingsService = settingsService;

            apiService.StatusRequested += ExposeStatus;
        }

        private void ExposeStatus(object sender, ApiRequestReceivedEventArgs e)
        {

        }

        [ApiMethod]
        public void GetStatus(IApiContext apiContext)
        {
            apiContext.Response = JObject.FromObject(CollectStatus());
        }

        private Status CollectStatus()
        {
            var status = new Status();

            status.OpenWindows.AddRange(GetOpenWindows());
            status.TiltWindows.AddRange(GetTiltWindows());
            status.ActiveActuators.AddRange(GetActuatorStatus());

            return status;
        }

        private List<WindowStatus> GetOpenWindows()
        {
            return _componentService.GetComponents<IWindow>()
                .Where(w => w.GetState().Equals(CasementStateId.Open))
                .Select(w => new WindowStatus { Id = w.Id, Caption = w.Settings.Caption }).ToList();
        }

        private List<WindowStatus> GetTiltWindows()
        {
            return _componentService.GetComponents<IWindow>()
                .Where(w => w.GetState().Equals(CasementStateId.Tilt))
                .Select(w => new WindowStatus { Id = w.Id, Caption = w.Settings.Caption }).ToList();
        }

        private List<ActuatorStatus> GetActuatorStatus()
        {
            var actuatorStatusList = new List<ActuatorStatus>();

            var actuators = _componentService.GetComponents<IActuator>();
            foreach (var actuator in actuators)
            {
                if (actuator.GetState().Equals(BinaryStateId.Off))
                {
                    continue;
                }

                var settings = _settingsService.GetSettings<ComponentSettings>(actuator.Id);
                var actuatorStatus = new ActuatorStatus { Id = actuator.Id, Caption = settings.Caption };
                actuatorStatusList.Add(actuatorStatus);
            }

            return actuatorStatusList;
        }
    }
}
