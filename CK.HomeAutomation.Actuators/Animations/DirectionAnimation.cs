using System;
using System.Collections.Generic;
using CK.HomeAutomation.Core.Timer;

namespace CK.HomeAutomation.Actuators.Animations
{
    public class DirectionAnimation : Animation
    {
        private CombinedBinaryStateActuators _actuator;
        private bool _isForward = true;
        private BinaryActuatorState _targetState;
        private TimeSpan _duration = TimeSpan.FromSeconds(1);
        
        public DirectionAnimation(IHomeAutomationTimer timer) : base(timer)
        {
        }

        public DirectionAnimation WithDuration(TimeSpan duration)
        {
            _duration = duration;
            return this;
        }

        public DirectionAnimation WithForwardDirection()
        {
            _isForward = true;
            return this;
        }

        public DirectionAnimation WithBackwardsDirection()
        {
            _isForward = false;
            return this;
        }

        public DirectionAnimation WithTargetOnState()
        {
            _targetState = BinaryActuatorState.On;
            return this;
        }

        public DirectionAnimation WithTargetOffState()
        {
            _targetState = BinaryActuatorState.Off;
            return this;
        }

        public DirectionAnimation WithActuator(CombinedBinaryStateActuators actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            _actuator = actuator;
            return this;
        }

        public override void Start()
        {
            Frames.Clear();
            
            var orderedActuators = new List<IBinaryStateOutputActuator>(_actuator.Actuators);
            if (!_isForward)
            {
                orderedActuators.Reverse();
            }

            double frameLength = _duration.TotalMilliseconds/orderedActuators.Count;

            if (_targetState == BinaryActuatorState.On)
            {
                WithFrame(CreateFrame(orderedActuators, BinaryActuatorState.Off).WithStartTime(TimeSpan.Zero));

                for (int i = 0; i < orderedActuators.Count; i++)
                {
                    var actuator = orderedActuators[i];
                    WithFrame(new Frame().WithAction(() => actuator.SetState(BinaryActuatorState.On)).WithStartTime(TimeSpan.FromMilliseconds(frameLength * i)));
                }

                WithFrame(CreateFrame(orderedActuators, BinaryActuatorState.On).WithStartTime(_duration));
            }
            else
            {
                WithFrame(CreateFrame(orderedActuators, BinaryActuatorState.Off).WithStartTime(TimeSpan.Zero));

                for (int i = 0; i < orderedActuators.Count; i++)
                {
                    var actuator = orderedActuators[i];
                    WithFrame(new Frame().WithAction(() => actuator.SetState(BinaryActuatorState.Off)).WithStartTime(TimeSpan.FromMilliseconds(frameLength * i)));
                }

                WithFrame(CreateFrame(orderedActuators, BinaryActuatorState.On).WithStartTime(_duration));
            }
            
            base.Start();
        }

        private Frame CreateFrame(ICollection<IBinaryStateOutputActuator> actuators, BinaryActuatorState state)
        {
            var frame = new Frame();

            foreach (var actuator in actuators)
            {
                frame.WithAction(() => actuator.SetState(state));
            }

            return frame;
        }
    }
}
