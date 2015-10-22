using HA4IoT.Actuators.Conditions;

namespace HA4IoT.Actuators.Tests
{
    public class FulfilledTestCondition : Condition
    {
        public FulfilledTestCondition()
        {
            WithExpression(() => ConditionState.Fulfilled);
        }
    }
}
