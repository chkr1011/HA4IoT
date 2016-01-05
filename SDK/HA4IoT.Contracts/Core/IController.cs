using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;
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

        void AddRoom(IRoom room);

        IRoom Room(RoomId id);

        IRoom CreateRoom(Enum id);
        
        Dictionary<ActuatorId, IActuator> Actuators { get; }
    }
}
