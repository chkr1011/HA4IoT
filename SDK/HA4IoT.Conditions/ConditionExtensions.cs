using System;
using HA4IoT.Contracts.Conditions;

namespace HA4IoT.Conditions
{
    public static class ConditionExtensions
    {
        public static bool IsFulfilled(this ICondition condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));

            return condition.Validate() == ConditionState.Fulfilled;
        }
    }
}
