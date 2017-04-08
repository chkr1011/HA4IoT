using System;

namespace HA4IoT.Contracts.Hardware.DeviceMessaging
{
    public class DeviceMessageReceivedEventArgs
    {
        public DeviceMessageReceivedEventArgs(DeviceMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            Message = message;
        }

        public DeviceMessage Message { get; }
    }
}
