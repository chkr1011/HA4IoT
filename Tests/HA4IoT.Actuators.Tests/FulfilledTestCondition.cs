using HA4IoT.Conditions;

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
