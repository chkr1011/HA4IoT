using System;
using System.Collections.Generic;
using HA4IoT.Actuators.Conditions;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Core.Timer;

namespace HA4IoT.Actuators.Automations
{
    public class Automation
    {
        private readonly ConditionsValidator _conditionsValidator;
        
        public IList<RelatedCondition> Conditions { get; } = new List<RelatedCondition>();
        public IList<Action> ActionsIfFulfilled { get; } = new List<Action>();
        public IList<Action> ActionsIfNotFulfilled { get; } = new List<Action>();

        public Automation(IHomeAutomationTimer timer)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            Timer = timer;
            _conditionsValidator = new ConditionsValidator(Conditions);
        }

        protected IHomeAutomationTimer Timer { get; }

        public Automation WithActionIfFulfilled(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            ActionsIfFulfilled.Add(action);
            return this;
        }

        public Automation WithActionIfNotFulfilled(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            ActionsIfNotFulfilled.Add(action);
            return this;
        }

        public Automation WithCondition(ConditionRelation relation, Condition condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));

            Conditions.Add(new RelatedCondition().WithCondition(condition).WithRelation(relation));
            return this;
        }

        public void Trigger()
        {
            if (_conditionsValidator.Validate() == ConditionState.Fulfilled)
            {
                foreach (var action in ActionsIfFulfilled)
                {
                    action();
                }
            }
            else
            {
                foreach (var action in ActionsIfNotFulfilled)
                {
                    action();
                }
            }            
        }

        public Automation WithAutoTrigger(TimeSpan interval)
        {
            Timer.Every(interval).Do(Trigger);
            return this;
        }

        public Automation WithButtonPressedShortTrigger(IButton button)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));

            button.PressedShort += (s, e) => Trigger();
            return this;
        }

        public Automation WithButtonPressedLongTrigger(IButton button)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));

            button.PressedLong += (s, e) => Trigger();
            return this;
        }

        public Automation WithMotionDetectedTrigger(IMotionDetector motionDetector)
        {
            if (motionDetector == null) throw new ArgumentNullException(nameof(motionDetector));

            motionDetector.MotionDetected += (s, e) => Trigger();
            return this;
        }

        public Automation WithMotionDetectionCompletedTrigger(IMotionDetector motionDetector)
        {
            if (motionDetector == null) throw new ArgumentNullException(nameof(motionDetector));

            motionDetector.DetectionCompleted += (s, e) => Trigger();
            return this;
        }

        public Automation WithBinaryStateOutputActuatorStateChangedTrigger(IBinaryStateOutputActuator actuator, BinaryActuatorState desiredState)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            actuator.StateChanged += (s, e) =>
            {
                if (actuator.GetState() == desiredState)
                {
                    Trigger();
                }
            };

            return this;
        }
    }
}
