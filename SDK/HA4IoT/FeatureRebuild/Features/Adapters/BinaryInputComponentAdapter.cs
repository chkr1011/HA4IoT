using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Hardware;
using HA4IoT.FeatureRebuild.Commands;
using HA4IoT.FeatureRebuild.Exceptions;
using HA4IoT.FeatureRebuild.Status;

namespace HA4IoT.FeatureRebuild.Features.Adapters
{
    public class BinaryInputComponentAdapter : IComponentAdapter
    {
        private readonly HashSet<IFeature> _features = new HashSet<IFeature>();

        private bool _isHigh;

        public BinaryInputComponentAdapter(IBinaryInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            input.StateChanged += DispatchChangedState;
        }

        public event EventHandler<StatusChangedEventArgs> StatusChanged;

        public void EnableFeature(IFeature feature)
        {
            if (feature == null) throw new ArgumentNullException(nameof(feature));

            _features.Add(feature);
        }

        public void InvokeCommand(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            throw new CommandNotSupportedException(command);
        }

        public IEnumerable<IFeature> GetFeatures()
        {
            return _features;
        }

        public IEnumerable<IStatus> GetStatus()
        {
            if (_features.OfType<DetectMotionFeature>().Any()) yield return new DetectMotionStatus { IsMotionDetected = _isHigh };
            if (_features.OfType<PressButtonFeature>().Any()) yield return new PressButtonStatus { IsPressed = _isHigh };
            if (_features.OfType<ActivateSwitchFeature>().Any()) yield return new ActivateSwitchStatus { IsActive = _isHigh };
        }

        private void DispatchChangedState(object sender, BinaryStateChangedEventArgs e)
        {
            var oldStatus = GetStatus().ToList();

            var newIsHigh = e.NewState == BinaryState.High;
            if (newIsHigh == _isHigh)
            {
                return;
            }

            _isHigh = newIsHigh;

            var newStatus = GetStatus().ToList();

            StatusChanged?.Invoke(this, new StatusChangedEventArgs(oldStatus, newStatus));
        }
    }
}
