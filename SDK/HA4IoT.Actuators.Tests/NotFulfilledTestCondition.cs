using HA4IoT.Conditions;

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
