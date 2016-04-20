using System;

namespace HA4IoT.Contracts.Logging
{
    public class MessageWithExceptionLoggedEventArgs : MessageLoggedEventArgs
    {
        public MessageWithExceptionLoggedEventArgs(string message, Exception exception) 
            : base(message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            Exception = exception;
        }

        public Exception Exception { get; private set; }
    }
}
