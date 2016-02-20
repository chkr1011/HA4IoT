using System;
using System.Net;

namespace HA4IoT.TraceViewer
{
    public class TraceItemReceivedEventArgs : EventArgs
    {
        public TraceItemReceivedEventArgs(IPAddress senderAddress, TraceItem traceItem)
        {
            if (senderAddress == null) throw new ArgumentNullException(nameof(senderAddress));
            if (traceItem == null) throw new ArgumentNullException(nameof(traceItem));

            SenderAddress = senderAddress;
            TraceItem = traceItem;
        }

        public IPAddress SenderAddress { get; }

        public TraceItem TraceItem { get; }
    }
}
