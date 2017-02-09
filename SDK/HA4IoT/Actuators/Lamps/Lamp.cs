using HA4IoT.Components;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Actuators.LogicalElements;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators.Lamps
{
    public class Lamp : ComponentBase, ILamp
    {
        private readonly PowerStateElement _powerStateElement;

        public Lamp(ComponentId id, IBinaryOutputComponentAdapter adapter) // TODO: Lamp adapter
            : base(id)
        {
            _powerStateElement = new PowerStateElement(adapter);

            TogglePowerStateAction = _powerStateElement.TogglePowerStateAction;
        }

        public IAction TogglePowerStateAction { get; }

        public override ComponentFeatureStateCollection GetState()
        {
            return new ComponentFeatureStateCollection()
                .With(_powerStateElement.GetState());
        }

        public override ComponentFeatureCollection GetFeatures()
        {
            return new ComponentFeatureCollection();
        }

        public override void InvokeCommand(ICommand command)
        {
            if (_powerStateElement.TryInvokeCommand(command))
            {
                return;
            }
        }

        public void ChangeState(IComponentFeatureState state, params IHardwareParameter[] parameters)
        {
        }

        public void ResetState()
        {
            _powerStateElement.ResetState();
        }
    }
}