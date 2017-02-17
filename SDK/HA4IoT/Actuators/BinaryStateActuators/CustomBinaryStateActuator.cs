using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Actuators.BinaryStateActuators
{
    public class CustomBinaryStateActuator : BinaryStateActuator
    {
        public CustomBinaryStateActuator(string id, IBinaryOutputAdapter endpoint)
            : base(id, endpoint)
        {
        }

        public override ComponentFeatureCollection GetFeatures()
        {
            return new ComponentFeatureCollection();
        }

        public override void InvokeCommand(ICommand command)
        {
        }
    }
}