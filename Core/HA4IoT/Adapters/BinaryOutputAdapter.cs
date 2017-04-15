using System;
using System.Linq;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Adapters
{
    public class BinaryOutputAdapter : IBinaryOutputAdapter, ILampAdapter
    {
        private readonly IBinaryOutput _output;

        public BinaryOutputAdapter(IBinaryOutput output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        public bool SupportsColor => false;

        public int ColorResolutionBits => 0;

        public void SetState(AdapterPowerState powerState, params IHardwareParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            var commit = !parameters.Any(p => p is IsPartOfPartialUpdateParameter);
            lock (_output)
            {
                _output.Write(powerState == AdapterPowerState.On ? BinaryState.High : BinaryState.Low, commit ? WriteBinaryStateMode.Commit : WriteBinaryStateMode.NoCommit);
            }
        }

        public void SetState(AdapterPowerState powerState, AdapterColor color, params IHardwareParameter[] hardwareParameters)
        {
            if (color != null)
            {
                throw new InvalidOperationException("Color is not supported.");
            }

            SetState(powerState, hardwareParameters);
        }
    }
}
