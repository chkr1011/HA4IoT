using System;
using HA4IoT.Conditions;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Conditions.Specialized
{
    public class BinaryInputStateCondition : Condition
    {
        public BinaryInputStateCondition(IBinaryInput input, BinaryState state)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            WithExpression(() => input.Read() == state);
        }
    }
}
