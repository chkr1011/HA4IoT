using System;
using System.Collections.Generic;
using HA4IoT.FeatureRebuild.Commands;
using HA4IoT.FeatureRebuild.Status;

namespace HA4IoT.FeatureRebuild.Features.Adapters
{
    public interface IComponentAdapter
    {
        event EventHandler<StatusChangedEventArgs> StatusChanged;

        void InvokeCommand(ICommand command);
        IEnumerable<IFeature> GetFeatures();
        IEnumerable<IStatus> GetStatus();
    }
}
