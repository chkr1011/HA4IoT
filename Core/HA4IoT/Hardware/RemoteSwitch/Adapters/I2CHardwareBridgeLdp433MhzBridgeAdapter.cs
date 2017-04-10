using System;
using HA4IoT.Hardware.I2C.I2CHardwareBridge;
using HA4IoT.Hardware.RemoteSwitch.Codes;

namespace HA4IoT.Hardware.RemoteSwitch.Adapters
{
    public class I2CHardwareBridgeLdp433MhzBridgeAdapter : ILdp433MhzBridgeAdapter
    {
        private readonly I2CHardwareBridge _i2CHardwareBridge;
        private readonly byte _pin;

        public I2CHardwareBridgeLdp433MhzBridgeAdapter(I2CHardwareBridge i2CHardwareBridge, byte pin)
        {
            _i2CHardwareBridge = i2CHardwareBridge ?? throw new ArgumentNullException(nameof(i2CHardwareBridge));
            _pin = pin;
        }

#pragma warning disable 0067
        public event EventHandler<Ldp433MhzCodeReceivedEventArgs> CodeReceived;
#pragma warning restore 0067

        public void SendCode(Lpd433MhzCode code)
        {
            var command = new SendLDP433MhzSignalCommand().WithPin(_pin).WithCode(code.Value).WithLength(code.Length).WithRepeats(code.Repeats);

            _i2CHardwareBridge.ExecuteCommand(command);
        }
    }
}