using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.FeatureRebuild.Commands;
using HA4IoT.FeatureRebuild.Exceptions;
using HA4IoT.FeatureRebuild.Status;

namespace HA4IoT.FeatureRebuild.Features.Adapters
{
    public class BinaryOutputComponentAdapter : IBinaryStateAdapter, IComponentAdapter
    {
        private readonly IBinaryOutput _output;

        private bool _isHigh;

        public BinaryOutputComponentAdapter(IBinaryOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            _output = output;
        }

        #region OLD

        public void TurnOn(params IHardwareParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            lock (_output)
            {
                bool commit = !parameters.Any(p => p is IsPartOfPartialUpdateParameter);
                _output.Write(BinaryState.High, commit);
            }
        }

        public void TurnOff(params IHardwareParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            lock (_output)
            {
                bool commit = !parameters.Any(p => p is IsPartOfPartialUpdateParameter);
                _output.Write(BinaryState.Low, commit);
            }
        }

        #endregion

        #region Feature rebuild

        public event EventHandler<StatusChangedEventArgs> StatusChanged;

        public void InvokeCommand(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            var turnOnCommand = command as TurnOnCommand;
            if (turnOnCommand != null)
            {
                InvokeTurnOnCommand(turnOnCommand);
                return;
            }

            var turnOffCommand = command as TurnOffCommand;
            if (turnOffCommand != null)
            {
                InvokeTurnOffCommand(turnOffCommand);
                return;
            }

            throw new CommandNotSupportedException(command);
        }

        public IEnumerable<IFeature> GetFeatures()
        {
            yield return new TurnOnFeature();
            yield return new TurnOffFeature();
        }

        public IEnumerable<IStatus> GetStatus()
        {
            yield return new TurnOnStatus { IsTurnedOn = _isHigh };
            yield return new TurnOffStatus { IsTurnedOff = !_isHigh };
        }

        private void InvokeTurnOnCommand(TurnOnCommand turnOnCommand)
        {
            if (_isHigh)
            {
                return;
            }

            var oldStatus = GetStatus().ToList();
            lock (_output)
            {
                _output.Write(BinaryState.High, !turnOnCommand.IsPartOfLogicalComponent);
                _isHigh = true;

                StatusChanged?.Invoke(this, new StatusChangedEventArgs(oldStatus, GetStatus().ToList()));
            }
        }

        private void InvokeTurnOffCommand(TurnOffCommand turnOffCommand)
        {
            if (!_isHigh)
            {
                return;
            }

            var oldStatus = GetStatus().ToList();
            lock (_output)
            {
                _output.Write(BinaryState.Low, !turnOffCommand.IsPartOfLogicalComponent);
                _isHigh = false;

                StatusChanged?.Invoke(this, new StatusChangedEventArgs(oldStatus, GetStatus().ToList()));
            }
        }

        #endregion
    }
}
