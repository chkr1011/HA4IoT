using System;
using Windows.Data.Json;
using CK.HomeAutomation.Networking;

namespace CK.HomeAutomation.Hardware.RemoteSwitch
{
    public class LPD433MhzSignalSender : ILPD433MhzSignalSender
    {
        private const byte ActionSendLpd433Signal = 2;

        private readonly int _address;
        private readonly II2cBusAccessor _i2CBus;
        
        public LPD433MhzSignalSender(II2cBusAccessor i2CBus, int address, IHttpRequestController httpApiController)
        {
            if (i2CBus == null) throw new ArgumentNullException(nameof(i2CBus));
            if (httpApiController == null) throw new ArgumentNullException(nameof(httpApiController));

            _i2CBus = i2CBus;
            _address = address;

            httpApiController.Handle(HttpMethod.Post, "433Mhz").WithRequiredJsonBody().Using(ApiPost);
        }

        public void Send(LPD433MhzCodeSequence codeSequence)
        {
            foreach (var code in codeSequence.Codes)
            {
                Send(code.Value, code.Length);
            }
        }

        private void ApiPost(HttpContext context)
        {
            JsonArray sequence = context.Request.JsonBody.GetNamedArray("sequence", null);
            if (sequence == null)
            {
                return;
            }

            var codeSequence = new LPD433MhzCodeSequence();
            foreach (IJsonValue item in sequence)
            {
                var code = item.GetObject();

                ulong value = (ulong)code.GetNamedNumber("value", 0);
                int length = (int)code.GetNamedNumber("length", 0);

                if (value == 0 || length == 0)
                {
                    throw new InvalidOperationException("Value or length is null.");
                }

                codeSequence.WithCode(new LPD433MhzCode(value, length));
            }

            Send(codeSequence);
        }

        private void Send(ulong code, int length)
        {
            var codeBuffer = BitConverter.GetBytes(code);

            var package = new byte[6];
            package[0] = ActionSendLpd433Signal;

            Array.Copy(codeBuffer, 0, package, 1, 4); // The code.
            package[5] = (byte)length;

            _i2CBus.Execute(_address, bus => bus.Write(package), false);
        }
    }
}
