using System;
using Windows.Data.Json;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Tests.Mockups
{
    public class TestMotionDetector : IMotionDetector
    {
        private readonly Trigger _detectionCompletedTrigger = new Trigger();
        private readonly Trigger _motionDetectedTrigger = new Trigger();

        public event EventHandler<MotionDetectorStateChangedEventArgs> StateChanged;

        public TestMotionDetector()
        {
            Settings = new ActuatorSettings(ActuatorIdFactory.EmptyId);
        }

        public ActuatorId Id { get; set; }
        public IActuatorSettings Settings { get; }
        public MotionDetectorState State { get; private set; } = MotionDetectorState.Idle;

        public JsonObject ExportConfigurationToJsonObject()
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

        public JsonObject ExportStatusToJsonObject()
        {
            return new JsonObject();
        }

        public void LoadSettings()
        {
        }

        public void ExposeToApi()
        {
            
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
            _motionDetectedTrigger.Execute();
        }

        public void FireDetectionCompleted()
        {
            State = MotionDetectorState.Idle;
            _motionDetectedTrigger.Execute();
        }
    }
}