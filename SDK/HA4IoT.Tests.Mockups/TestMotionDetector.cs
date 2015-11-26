using System;
using Windows.Data.Json;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Tests.Mockups
{
    public class TestMotionDetector : IMotionDetector
    {
        public event EventHandler<ActuatorIsEnabledChangedEventArgs> IsEnabledChanged;
        public event EventHandler MotionDetected;
        public event EventHandler DetectionCompleted;
        public event EventHandler<MotionDetectorStateChangedEventArgs> StateChanged;
        
        public ActuatorId Id { get; set; }
        public bool IsEnabled { get; }
        public MotionDetectorState State { get; private set; } = MotionDetectorState.Idle;

        public void SetState(MotionDetectorState newState)
        {
            var oldState = State;
            State = newState;

            StateChanged?.Invoke(this, new MotionDetectorStateChangedEventArgs(oldState, newState));
        }

        public MotionDetectorState GetState()
        {
            return State;
        }

        public bool IsMotionDetected { get; set; }

        public void WalkIntoMotionDetector()
        {
            State = MotionDetectorState.MotionDetected;
            MotionDetected?.Invoke(this, EventArgs.Empty);
        }

        public void FireDetectionCompleted()
        {
            State = MotionDetectorState.Idle;
            DetectionCompleted?.Invoke(this, EventArgs.Empty);
        }

        public JsonObject GetStatus()
        {
            return new JsonObject();
        }
    }
}
