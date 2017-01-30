using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using HA4IoT.FeatureRebuild.Commands;
using HA4IoT.FeatureRebuild.Features;
using HA4IoT.FeatureRebuild.Features.Adapters;
using HA4IoT.FeatureRebuild.Status;
using Newtonsoft.Json;
using ComponentSettings = HA4IoT.Components.ComponentSettings;

namespace HA4IoT.FeatureRebuild
{
    public class Component
    {
        private IComponentAdapter _adapter;

        public Component(ComponentId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = id;
        }

        public ComponentId Id { get; }

        public ComponentSettings Settings { get; } = new ComponentSettings();

        public IComponentAdapter Adapter
        {
            get
            {
                return _adapter;
            }

            set
            {
                if (_adapter != null)
                {
                    _adapter.StatusChanged -= OnAdapterStatusChanged;
                }
                
                _adapter = value;

                if (_adapter != null)
                {
                    _adapter.StatusChanged += OnAdapterStatusChanged;
                }
            }
        }

        private void OnAdapterStatusChanged(object sender, StatusChangedEventArgs statusChangedEventArgs)
        {
            var status = statusChangedEventArgs.NewStatus.ToDictionary(i => i.GetType().Name, i => i);
            var statusText = JsonConvert.SerializeObject(status);
            Log.Info($"Component '{Id}' changed status to: {statusText}");

            StatusChanged?.Invoke(this, statusChangedEventArgs);
        }

        public event EventHandler<StatusChangedEventArgs> StatusChanged;

        public IEnumerable<IFeature> GetFeatures()
        {
            return _adapter?.GetFeatures() ?? new IFeature[0];
        }

        public IEnumerable<IStatus> GetStatus()
        {
            return _adapter?.GetStatus() ?? new IStatus[0];
        }

        public TStatus GetStatus<TStatus>()
        {
            return GetStatus().OfType<TStatus>().SingleOrDefault();
        }

        public void InvokeCommand(ICommand command)
        {
            _adapter?.InvokeCommand(command);
        }
    }
}
