using CK.HomeAutomation.Actuators.Conditions;

namespace CK.HomeAutomation.Actuators.Tests
{
    public class FulfilledTestCondition : Condition
    {
        public FulfilledTestCondition()
        {
            WithExpression(() => ConditionState.Fulfilled);
        }
    }
}
