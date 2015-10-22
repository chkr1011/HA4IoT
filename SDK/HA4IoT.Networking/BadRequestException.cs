using System;

namespace CK.HomeAutomation.Networking
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }
}
