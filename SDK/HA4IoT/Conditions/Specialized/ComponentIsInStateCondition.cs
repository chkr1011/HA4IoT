using System;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Conditions.Specialized
{
    public class ComponentIsInStateCondition : Condition
    {
        public ComponentIsInStateCondition(IComponent component, ComponentState state)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (state == null) throw new ArgumentNullException(nameof(state));
            
            WithExpression(() => component.GetState().Equals(state));
        }        
    }
}
