using System;
using Windows.Data.Json;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Triggers;
using HA4IoT.Core.Settings;

namespace HA4IoT.Tests.Mockups
{
    public class TestMotionDetector : IMotionDetector
    {
        private readonly Trigger _detectionCompletedTrigger = new Trigger();
        private readonly Trigger _motionDetectedTrigger = new Trigger();

        public event EventHandler<MotionDetectorStateChangedEventArgs> StateChanged;

        public TestMotionDetector()
        {
            Settings = new SettingsContainer(string.Empty);
            GeneralSettingsWrapper = new ActuatorSettingsWrapper(Settings);
        }

        public ActuatorId Id { get; set; }
        public ISettingsContainer Settings { get; }
        public IActuatorSettingsWrapper GeneralSettingsWrapper { get; }
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

        public void ExposeToApi(IApiController apiController)
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