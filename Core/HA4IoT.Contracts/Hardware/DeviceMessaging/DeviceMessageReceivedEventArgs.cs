using System;

namespace HA4IoT.Contracts.Hardware.DeviceMessaging
{
    public class DeviceMessageReceivedEventArgs
    {
        public DeviceMessageReceivedEventArgs(DeviceMessage message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public DeviceMessage Message { get; }
    }
}
