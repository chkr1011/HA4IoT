using System;
using System.Text;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.DeviceMessaging;

namespace HA4IoT.Controller.Main.Main.Rooms
{
    internal sealed class DeviceBinaryInput : IBinaryInput
    {
        private BinaryState _state = BinaryState.Low;

        public DeviceBinaryInput(string topic, IDeviceMessageBrokerService messageBroker)
        {
            messageBroker.Subscribe(topic, MqttCallback);
        }

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        public BinaryState Read()
        {
            return _state;
        }

        private void MqttCallback(DeviceMessage deviceMessage)
        {
            var oldState = _state;

            var payload = Encoding.UTF8.GetString(deviceMessage.Payload);
            if (payload == "pressed")
            {
                _state = BinaryState.High;
            }
            else
            {
                _state = BinaryState.Low;
            }

            if (_state == oldState)
            {
                return;
            }

            StateChanged?.Invoke(this, new BinaryStateChangedEventArgs(oldState, _state));
        }
    }
}
