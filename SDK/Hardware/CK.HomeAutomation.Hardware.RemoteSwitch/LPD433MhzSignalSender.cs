using System;
using Windows.Data.Json;
using CK.HomeAutomation.Hardware.I2CHardwareBridge;
using CK.HomeAutomation.Networking;

namespace CK.HomeAutomation.Hardware.RemoteSwitch
{
    public class LPD433MHzSignalSender : ILPD433MHzSignalSender
    {
        private readonly I2CHardwareBridge.I2CHardwareBridge _i2CHardwareBridge;
        private readonly byte _pin;

        public LPD433MHzSignalSender(I2CHardwareBridge.I2CHardwareBridge i2CHardwareBridge, byte pin, IHttpRequestController httpApiController)
        {
            if (i2CHardwareBridge == null) throw new ArgumentNullException(nameof(i2CHardwareBridge));
            if (httpApiController == null) throw new ArgumentNullException(nameof(httpApiController));

            _i2CHardwareBridge = i2CHardwareBridge;
            _pin = pin;
            httpApiController.Handle(HttpMethod.Post, "433MHz").WithRequiredJsonBody().Using(ApiPost);
        }

        private void ApiPost(HttpContext context)
        {
            JsonArray sequence = context.Request.JsonBody.GetNamedArray("sequence", new JsonArray());
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

                if (value == 0 || length == 0)
                {
                    throw new InvalidOperationException("Value or length is null.");
                }

                codeSequence.WithCode(new LPD433MHzCode(value, length));
            }

            Send(codeSequence);
        }

        public void Send(LPD433MHzCodeSequence codeSequence)
        {
            foreach (var code in codeSequence.Codes)
            {
                Send(code.Value, code.Length);
            }
        }

        private void Send(uint code, byte length)
        {
            var command = new SendLDP433MhzSignalCommand().WithCode(code).WithLength(length).WithPin(_pin);
            _i2CHardwareBridge.ExecuteCommand(command);
        }
    }
}
