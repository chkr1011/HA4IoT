using System;
using CK.HomeAutomation.Networking;

namespace CK.HomeAutomation.Hardware.RemoteSwitch
{
    public class Wireless433MhzSignalSender
    {
        private readonly int _address;
        private readonly II2cBusAccessor _i2CBus;
        
        public Wireless433MhzSignalSender(II2cBusAccessor i2CBus, int address, IHttpRequestController httpApiController)
        {
            if (i2CBus == null) throw new ArgumentNullException(nameof(i2CBus));
            if (httpApiController == null) throw new ArgumentNullException(nameof(httpApiController));

            _i2CBus = i2CBus;
            _address = address;

            httpApiController.Handle(HttpMethod.Post, "433Mhz").WithRequiredJsonBody().Using(ApiPost);
        }

        public void Send(RemoteSwitchCode code)
        {
            Send(code.Code, code.Length);
        }

        private void ApiPost(HttpContext context)
        {
            ulong code = (ulong)context.Request.JsonBody.GetNamedNumber("code", 0);
            int length = (int)context.Request.JsonBody.GetNamedNumber("length", 0);

            if (code == 0 || length == 0)
            {
                throw new InvalidOperationException("Code or length is null.");
            }

            Send(code, length);
        }

        private void Send(ulong code, int length)
        {
            var codeBuffer = BitConverter.GetBytes(code);

            var package = new byte[6];
            package[0] = 2; // Action "send 433Mhz signal"
            Array.Copy(codeBuffer, 0, package, 1, 4); // The code.
            package[5] = (byte)length; // The length.

            _i2CBus.Execute(_address, bus => bus.Write(package), false);
        }
    }
}
