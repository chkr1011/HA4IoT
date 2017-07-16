using System;
using HA4IoT.Contracts.Hardware.RemoteSockets;
using HA4IoT.Contracts.Hardware.RemoteSockets.Adapters;
using HA4IoT.Contracts.Hardware.RemoteSockets.Codes;

namespace HA4IoT.Hardware.Drivers.I2CHardwareBridge
{
    public sealed class I2CHardwareBridgeLdp433MhzBridgeAdapter : ILdp433MhzBridgeAdapter
    {
        private readonly I2CHardwareBridge _i2CHardwareBridge;
        private readonly byte _pin;

        public I2CHardwareBridgeLdp433MhzBridgeAdapter(string id, I2CHardwareBridge i2CHardwareBridge, byte pin)
        {
            _i2CHardwareBridge = i2CHardwareBridge ?? throw new ArgumentNullException(nameof(i2CHardwareBridge));
            _pin = pin;
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        public string Id { get; }

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