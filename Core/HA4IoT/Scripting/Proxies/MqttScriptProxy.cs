using System;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Hardware.Services;
using HA4IoT.Contracts.Scripting;
using MoonSharp.Interpreter;

namespace HA4IoT.Scripting.Proxies
{
    public class MqttScriptProxy : IScriptProxy
    {
        private readonly IScriptingSession _scriptingSession;
        private readonly IDeviceMessageBrokerService _deviceMessageBrokerService;

        [MoonSharpHidden]
        public MqttScriptProxy(IDeviceMessageBrokerService deviceMessageBrokerService, IScriptingSession scriptingSession)
        {
            _scriptingSession = scriptingSession ?? throw new ArgumentNullException(nameof(scriptingSession));
            _deviceMessageBrokerService = deviceMessageBrokerService ?? throw new ArgumentNullException(nameof(deviceMessageBrokerService));
        }

        [MoonSharpHidden]
        public string Name => "mqtt";

        public void Publish(string topic, string message)
        {
            _deviceMessageBrokerService.Publish(topic, message, MqttQosLevel.AtMostOnce);
        }

        public void Subscribe(string topic, string callbackFunctionName)
        {
            _deviceMessageBrokerService.Subscribe(topic, _ =>
            {
                _scriptingSession.Execute(callbackFunctionName);
            });
        }
    }
}