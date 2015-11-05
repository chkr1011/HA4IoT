using HA4IoT.Actuators.Conditions;

namespace HA4IoT.Actuators.Tests
{
    public class NotFulfilledTestCondition : Condition
    {
        public NotFulfilledTestCondition()
        {
            WithExpression(() => ConditionState.NotFulfilled);
        }
    }
}
