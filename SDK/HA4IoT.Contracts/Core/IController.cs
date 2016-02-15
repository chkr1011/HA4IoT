using System.Collections.Generic;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Networking;

namespace HA4IoT.Contracts.Core
{
    public interface IController
    {
        ILogger Logger { get; }
        IHttpRequestController HttpApiController { get; }
        IHomeAutomationTimer Timer { get; }

        void AddDevice(IDevice device);

        TDevice Device<TDevice>(DeviceId id) where TDevice : IDevice;

        TDevice Device<TDevice>() where TDevice : IDevice;

        IList<TDevice> Devices<TDevice>() where TDevice : IDevice;

        void AddArea(IArea room);

        IArea Area(AreaId id);

        IList<IArea> Areas();

        void AddActuator(IActuator actuator);

        TActuator Actuator<TActuator>(ActuatorId id) where TActuator : IActuator;

        IList<IActuator> Actuators();
    }
}
