using System;
using Windows.Data.Json;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Tests.Mockups
{
    public class TestMotionDetector : IMotionDetector
    {
        private readonly Trigger _motionDetectedTrigger = new Trigger();
        private readonly Trigger _detectionCompletedTrigger = new Trigger();

        public event EventHandler<ActuatorIsEnabledChangedEventArgs> IsEnabledChanged;
        public event EventHandler<MotionDetectorStateChangedEventArgs> StateChanged;
        
        public ActuatorId Id { get; set; }
        public bool IsEnabled { get; }

        public MotionDetectorState State { get; private set; } = MotionDetectorState.Idle;
        public bool IsMotionDetected { get; set; }

        public JsonObject GetConfigurationForApi()
        {
            return new JsonObject();
        }

        public MotionDetectorState GetState()
        {
            return State;
        }

        public ITrigger GetMotionDetectedTrigger()
        {
            return _motionDetectedTrigger;
        }

        public ITrigger GetDetectionCompletedTrigger()
        {
            return _detectionCompletedTrigger;
        }

        public void SetState(MotionDetectorState newState)
        {
            var oldState = State;
            State = newState;

            StateChanged?.Invoke(this, new MotionDetectorStateChangedEventArgs(oldState, newState));
        }

        public void WalkIntoMotionDetector()
        {
            State = MotionDetectorState.MotionDetected;
            _motionDetectedTrigger.Invoke();
        }

        public void FireDetectionCompleted()
        {
            State = MotionDetectorState.Idle;
            _motionDetectedTrigger.Invoke();
        }

        public JsonObject GetStatusForApi()
        {
            return new JsonObject();
        }
    }
}
