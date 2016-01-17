using System.Collections.Generic;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Contracts.WeatherStation;
using HA4IoT.Networking;

namespace HA4IoT.Contracts.Core
{
    public interface IController
    {
        INotificationHandler Logger { get; }
        IHttpRequestController HttpApiController { get; }
        IHomeAutomationTimer Timer { get; }
        IWeatherStation WeatherStation { get; }

        void AddDevice(IDevice device);

        TDevice Device<TDevice>(DeviceId id) where TDevice : IDevice;

        IList<TDevice> Devices<TDevice>() where TDevice : IDevice;

        void AddRoom(IRoom room);

        IRoom Room(RoomId id);

        IList<IRoom> Rooms();

        void AddActuator(IActuator actuator);

        IList<IActuator> Actuators();
    }
}
