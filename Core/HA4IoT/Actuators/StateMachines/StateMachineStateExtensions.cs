using System;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators.StateMachines
{
    public static class StateMachineStateExtensions
    {
        public const string OffStateId = "Off";
        public const string OnStateId = "On";

        public static StateMachineState WithLowBinaryOutput(this StateMachineState stateMachineState, IBinaryOutput output)
        {
            if (stateMachineState == null) throw new ArgumentNullException(nameof(stateMachineState));
            if (output == null) throw new ArgumentNullException(nameof(output));

            return stateMachineState.WithBinaryOutput(output, BinaryState.Low);
        }

        public static StateMachineState WithHighBinaryOutput(this StateMachineState stateMachineState, IBinaryOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            return stateMachineState.WithBinaryOutput(output, BinaryState.High);
        }
    }
}
