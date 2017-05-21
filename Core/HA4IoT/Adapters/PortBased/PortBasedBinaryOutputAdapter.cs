using System;
using System.Linq;
using System.Threading.Tasks;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Adapters.PortBased
{
    public class PortBasedBinaryOutputAdapter : IBinaryOutputAdapter, ILampAdapter
    {
        private readonly IBinaryOutput _output;

        public PortBasedBinaryOutputAdapter(IBinaryOutput output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        public bool SupportsColor => false;

        public int ColorResolutionBits => 0;

        public Task SetState(AdapterPowerState powerState, params IHardwareParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            var commit = !parameters.Any(p => p is IsPartOfPartialUpdateParameter);
            lock (_output)
            {
                _output.Write(powerState == AdapterPowerState.On ? BinaryState.High : BinaryState.Low, commit ? WriteBinaryStateMode.Commit : WriteBinaryStateMode.NoCommit);
            }

            return Task.FromResult(0);
        }

        public Task SetState(AdapterPowerState powerState, AdapterColor color, params IHardwareParameter[] hardwareParameters)
        {
            if (color != null)
            {
                throw new InvalidOperationException("Color is not supported.");
            }

            SetState(powerState, hardwareParameters);

            return Task.FromResult(0);
        }
    }
}
