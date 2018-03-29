using System;
using System.Text;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Hardware.Mqtt;

namespace HA4IoT.Controller.Main.Main.Rooms
{
    internal sealed class DeviceBinaryOutput : IBinaryOutput
    {
        private readonly IDeviceMessageBrokerService _messageBroker;
        private readonly string _topic;

        private BinaryState _state = BinaryState.Low;

        public DeviceBinaryOutput(string topic, IDeviceMessageBrokerService messageBroker)
        {
            _messageBroker = messageBroker ?? throw new ArgumentNullException(nameof(messageBroker));

            _topic = topic;
        }

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        public BinaryState Read()
        {
            return _state;
        }

        public void Write(BinaryState state, WriteBinaryStateMode mode = WriteBinaryStateMode.Commit)
        {
            if (_state == state)
            {
                return;
            }

            var payload = "off";
            if (state == BinaryState.High)
            {
                payload = "on";
            }

            _messageBroker.Publish(_topic, Encoding.UTF8.GetBytes(payload), MqttQosLevel.AtMostOnce, true);
            StateChanged?.Invoke(this, new BinaryStateChangedEventArgs(_state, state));

            _state = state;
        }
    }
}
