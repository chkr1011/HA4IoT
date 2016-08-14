using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Hardware.I2CHardwareBridge;

namespace HA4IoT.Hardware.RemoteSwitch
{
    public class LPD433MHzSignalSender : ILPD433MHzSignalSender
    {
        private readonly I2CHardwareBridge.I2CHardwareBridge _i2CHardwareBridge;
        private readonly byte _pin;

        public LPD433MHzSignalSender(I2CHardwareBridge.I2CHardwareBridge i2CHardwareBridge, byte pin, IApiService apiController)
        {
            if (i2CHardwareBridge == null) throw new ArgumentNullException(nameof(i2CHardwareBridge));
            if (apiController == null) throw new ArgumentNullException(nameof(apiController));

            _i2CHardwareBridge = i2CHardwareBridge;
            _pin = pin;

            apiController.RouteCommand("433MHz", ApiPost);
        }

        private void ApiPost(IApiContext apiContext)
        {
            JsonArray sequence = apiContext.Request.GetNamedArray("sequence", new JsonArray());
            if (sequence.Count == 0)
            {
                return;
            }

            var codeSequence = new LPD433MHzCodeSequence();
            foreach (IJsonValue item in sequence)
            {
                var code = item.GetObject();

                uint value = (uint)code.GetNamedNumber("value", 0);
                byte length = (byte)code.GetNamedNumber("length", 0);
                byte repeats = (byte)code.GetNamedNumber("repeats", 1);

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
