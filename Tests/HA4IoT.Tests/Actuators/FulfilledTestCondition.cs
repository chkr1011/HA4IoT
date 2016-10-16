using HA4IoT.Conditions;
using HA4IoT.Contracts.Conditions;

namespace HA4IoT.Tests.Actuators
{
    public class FulfilledTestCondition : Condition
    {
        public FulfilledTestCondition()
        {
            WithExpression(() => ConditionState.Fulfilled);
        }
    }
}
