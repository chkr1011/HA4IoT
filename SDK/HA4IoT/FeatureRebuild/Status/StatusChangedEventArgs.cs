using System;
using System.Collections.Generic;

namespace HA4IoT.FeatureRebuild.Status
{
    public class StatusChangedEventArgs : EventArgs
    {
        public StatusChangedEventArgs(IList<IStatus> oldStatus, IList<IStatus> newStatus)
        {
            if (oldStatus == null) throw new ArgumentNullException(nameof(oldStatus));
            if (newStatus == null) throw new ArgumentNullException(nameof(newStatus));

            OldStatus = oldStatus;
            NewStatus = newStatus;
        }

        public IList<IStatus> OldStatus { get; }

        public IList<IStatus> NewStatus { get; }
    }
}
