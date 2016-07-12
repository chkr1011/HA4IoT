using HA4IoT.Conditions;
using HA4IoT.Contracts.Conditions;

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
