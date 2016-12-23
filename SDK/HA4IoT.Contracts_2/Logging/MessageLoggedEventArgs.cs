using System;

namespace HA4IoT.Contracts.Logging
{
    public class MessageLoggedEventArgs : EventArgs
    {
        public MessageLoggedEventArgs(string message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            Message = message;
        }

        public string Message { get; private set; }
    }
}
