using System;

namespace HA4IoT.Networking.Http
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }
}
