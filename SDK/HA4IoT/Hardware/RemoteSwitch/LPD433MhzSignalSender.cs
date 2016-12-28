using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Hardware.I2CHardwareBridge;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Hardware.RemoteSwitch
{
    public class LPD433MHzSignalSender : ILPD433MHzSignalSender
    {
        private readonly I2CHardwareBridge.I2CHardwareBridge _i2CHardwareBridge;
        private readonly byte _pin;

        public LPD433MHzSignalSender(I2CHardwareBridge.I2CHardwareBridge i2CHardwareBridge, byte pin, IApiDispatcherService apiController)
        {
            if (i2CHardwareBridge == null) throw new ArgumentNullException(nameof(i2CHardwareBridge));
            if (apiController == null) throw new ArgumentNullException(nameof(apiController));

            _i2CHardwareBridge = i2CHardwareBridge;
            _pin = pin;

            apiController.Route("433MHz", ApiPost);
        }

        private void ApiPost(IApiContext apiContext)
        {
            var sequence = apiContext.Parameter["sequence"].ToObject<JArray>();
            if (sequence.Count == 0)
            {
                return;
            }

            var codeSequence = new LPD433MHzCodeSequence();
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

                codeSequence.WithCode(new LPD433MHzCode(value, length, repeats));
            }

            Send(codeSequence);
        }

        public void Send(LPD433MHzCodeSequence codeSequence)
        {
            foreach (var code in codeSequence.Codes)
            {
                Send(code);
            }
        }

        private void Send(LPD433MHzCode code)
        {
            var command = new SendLDP433MhzSignalCommand().WithPin(_pin).WithCode(code.Value).WithLength(code.Length).WithRepeats(code.Repeats);
            _i2CHardwareBridge.ExecuteCommand(command);
        }
    }
}
