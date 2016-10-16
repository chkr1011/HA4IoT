using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Telemetry
{
    public abstract class ActuatorMonitor
    {
        private readonly IComponentService _componentService;

        protected ActuatorMonitor(IComponentService componentService)
        {
            if (componentService == null) throw new ArgumentNullException(nameof(componentService));

            _componentService = componentService;
        }

        public void Connect()
        {
            foreach (var component in _componentService.GetComponents())
            {
                OnComponentConnecting(component);

                var stateMachineOutput = component as IStateMachine;
                if (stateMachineOutput != null)
                {
                    HandleStateMachineOutputActuator(stateMachineOutput);
                    continue;
                }

                var sensor = component as INumericValueSensor;
                if (sensor != null)
                {
                    OnSensorValueChanged(sensor, sensor.GetCurrentNumericValue());
                    sensor.StateChanged += (s, e) =>
                    {
                        OnSensorValueChanged(sensor, sensor.GetCurrentNumericValue());
                    };

                    continue;
                }

                var motionDetector = component as IMotionDetector;
                if (motionDetector != null)
                {
                    motionDetector.GetMotionDetectedTrigger().Attach(() => OnMotionDetected(motionDetector));
                    continue;
                }

                var button = component as IButton;
                if (button != null)
                {
                    button.GetPressedShortlyTrigger().Attach(() => OnButtonPressed(button, ButtonPressedDuration.Short));
                    button.GetPressedLongTrigger().Attach(() => OnButtonPressed(button, ButtonPressedDuration.Long));
                }
            }
        }

        protected virtual void OnComponentConnecting(IComponent component)
        {
        }

        protected virtual void OnMotionDetected(IMotionDetector motionDetector)
        {
        }

        protected virtual void OnButtonPressed(IButton button, ButtonPressedDuration duration)
        {
        }

        protected virtual void OnStateMachineStateChanged(IStateMachine stateMachine, ComponentState newState)
        {
        }

        protected virtual void OnSensorValueChanged(INumericValueSensor sensor, float newValue)
        {
        }

        private void HandleStateMachineOutputActuator(IStateMachine stateMachine)
        {
            OnStateMachineStateChanged(stateMachine, stateMachine.GetState());

            stateMachine.StateChanged += (s, e) =>
            {
                OnStateMachineStateChanged(stateMachine, e.NewState);
            };
        }
    }
}
