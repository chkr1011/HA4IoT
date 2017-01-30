using System;
using System.Collections.Generic;
using System.Linq;

namespace HA4IoT.FeatureRebuild.Status
{
    public static class StatusChangedEventArgsExtensions
    {
        public static TStatus GetOldStatus<TStatus>(this StatusChangedEventArgs eventArgs) where TStatus : IStatus
        {
            if (eventArgs == null) throw new ArgumentNullException(nameof(eventArgs));

            return GetStatus<TStatus>(eventArgs.OldStatus);
        }

        public static TStatus GetNewStatus<TStatus>(this StatusChangedEventArgs eventArgs) where TStatus : IStatus
        {
            if (eventArgs == null) throw new ArgumentNullException(nameof(eventArgs));

            return GetStatus<TStatus>(eventArgs.NewStatus);
        }

        public static TStatus GetStatus<TStatus>(IEnumerable<IStatus> source) where TStatus : IStatus
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return source.OfType<TStatus>().Single();
        }
    }
}
