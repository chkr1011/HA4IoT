using CK.HomeAutomation.Actuators.Conditions;

namespace CK.HomeAutomation.Actuators.Tests
{
    public class NotFulfilledTestCondition : Condition
    {
        public NotFulfilledTestCondition()
        {
            WithExpression(() => ConditionState.NotFulfilled);
        }
    }
}
