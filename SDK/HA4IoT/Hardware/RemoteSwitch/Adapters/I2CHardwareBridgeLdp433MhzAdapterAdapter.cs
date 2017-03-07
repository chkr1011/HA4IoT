using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Hardware.I2C.I2CHardwareBridge;
using HA4IoT.Hardware.RemoteSwitch.Codes;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Hardware.RemoteSwitch.Adapters
{
    public class I2CHardwareBridgeLdp433MhzAdapterAdapter : ILdp433MhzAdapter
    {
        private readonly I2CHardwareBridge _i2CHardwareBridge;
        private readonly byte _pin;

        public I2CHardwareBridgeLdp433MhzAdapterAdapter(I2CHardwareBridge i2CHardwareBridge, byte pin, IApiDispatcherService apiController)
        {
            if (i2CHardwareBridge == null) throw new ArgumentNullException(nameof(i2CHardwareBridge));
            if (apiController == null) throw new ArgumentNullException(nameof(apiController));

            _i2CHardwareBridge = i2CHardwareBridge;
            _pin = pin;

            apiController.Route("433MHz", ApiPost);
        }

        public event EventHandler<Ldp433MhzCodeSequenceReceivedEventArgs> CodeSequenceReceived;

        private void ApiPost(IApiContext apiContext)
        {
            var sequence = apiContext.Parameter["sequence"].ToObject<JArray>();
            if (sequence.Count == 0)
            {
                return;
            }

            var codeSequence = new Lpd433MhzCodeSequence();
            foreach (var item in sequence)
            {
                var code = item.ToObject<JObject>();

                var value = (uint)code["value"];
                var length = (byte)code["length"];
                var repeats = (byte)code["repeats"];

                if (value == 0 || length == 0)
                {
                    throw new InvalidOperationException("Value or length is null.");
                }

                codeSequence.WithCode(new Lpd433MhzCode(value, length, repeats));
            }

            SendCodeSequence(codeSequence);
        }

        private void Send(Lpd433MhzCode code)
        {
            var command = new SendLDP433MhzSignalCommand().WithPin(_pin).WithCode(code.Value).WithLength(code.Length).WithRepeats(code.Repeats);

            _i2CHardwareBridge.ExecuteCommand(command);
        }

        public void SendCodeSequence(Lpd433MhzCodeSequence codeSequence)
        {
            foreach (var code in codeSequence.Codes)
            {
                Send(code);
            }
        }
    }
}
