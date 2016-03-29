using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Telemetry
{
    public abstract class ActuatorMonitor
    { 
        public void ConnectActuators(IController controller)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            foreach (var actuator in controller.GetActuators<IActuator>())
            {
                OnActuatorConnecting(actuator);

                var stateMachineOutput = actuator as IStateMachine;
                if (stateMachineOutput != null)
                {
                    HandleStateMachineOutputActuator(stateMachineOutput);
                    continue;
                }

                var sensor = actuator as ISingleValueSensorActuator;
                if (sensor != null)
                {
                    OnSensorValueChanged(sensor, sensor.GetValue());
                    sensor.ValueChanged += (s, e) =>
                    {
                        OnSensorValueChanged(sensor, e.NewValue);
                    };

                    continue;
                }

                var motionDetector = actuator as IMotionDetector;
                if (motionDetector != null)
                {
                    motionDetector.GetMotionDetectedTrigger().Attach(() => OnMotionDetected(motionDetector));
                    continue;
                }

                var button = actuator as IButton;
                if (button != null)
                {
                    button.GetPressedShortlyTrigger().Attach(() => OnButtonPressed(button, ButtonPressedDuration.Short));
                    button.GetPressedLongTrigger().Attach(() => OnButtonPressed(button, ButtonPressedDuration.Long));
                }
            }
        }

        protected virtual void OnActuatorConnecting(IActuator actuator)
        {
        }

        protected virtual void OnMotionDetected(IMotionDetector motionDetector)
        {
        }

        protected virtual void OnButtonPressed(IButton button, ButtonPressedDuration duration)
        {
        }

        protected virtual void OnStateMachineStateChanged(IStateMachine stateMachine, StateMachineStateId newState)
        {
        }

        protected virtual void OnSensorValueChanged(ISingleValueSensorActuator sensor, float newValue)
        {
        }

        private void HandleStateMachineOutputActuator(IStateMachine stateMachine)
        {
            OnStateMachineStateChanged(stateMachine, stateMachine.GetActiveState());

            stateMachine.ActiveStateChanged += (s, e) =>
            {
                OnStateMachineStateChanged(stateMachine, e.NewValue);
            };
        }
    }
}
