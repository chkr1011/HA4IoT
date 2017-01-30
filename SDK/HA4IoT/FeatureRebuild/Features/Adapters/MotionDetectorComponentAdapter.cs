using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Hardware;
using HA4IoT.FeatureRebuild.Commands;
using HA4IoT.FeatureRebuild.Exceptions;
using HA4IoT.FeatureRebuild.Status;

namespace HA4IoT.FeatureRebuild.Features.Adapters
{
    public class MotionDetectorComponentAdapter : IComponentAdapter
    {
        private readonly IBinaryInput _input;

        private bool _isDetected;

        public MotionDetectorComponentAdapter(IBinaryInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            _input = input;
            _input.StateChanged += DispatchChangedState;
        }

        public event EventHandler<StatusChangedEventArgs> StatusChanged;

        public void InvokeCommand(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            throw new CommandNotSupportedException(command);
        }

        public IEnumerable<IFeature> GetFeatures()
        {
            yield return  new DetectMotionFeature();
        }

        public IEnumerable<IStatus> GetStatus()
        {
            yield return new DetectMotionStatus { IsMotionDetected = _isDetected };
        }

        private void DispatchChangedState(object sender, BinaryStateChangedEventArgs e)
        {
            var newValue = _input.Read() == BinaryState.High;
            if (newValue == _isDetected)
            {
                return;
            }

            var oldStatus = GetStatus().ToList();
            _isDetected = newValue;
            var newStatus = GetStatus().ToList();

            StatusChanged?.Invoke(this, new StatusChangedEventArgs(oldStatus, newStatus));
        }
    }
}
