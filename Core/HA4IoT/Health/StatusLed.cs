using System;
using System.Diagnostics;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Health
{
    public class StatusLed
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private readonly IBinaryOutput _output;

        private bool _isOn;
        
        public StatusLed(IBinaryOutput output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _output.Write(BinaryState.Low);
        }

        public void Update()
        {
            if (!_isOn && _stopwatch.ElapsedMilliseconds > 2000)
            {
                _output.Write(BinaryState.High);

                _isOn = true;
                _stopwatch.Restart();
            }
            else if (_isOn && _stopwatch.ElapsedMilliseconds > 200)
            {
                _output.Write(BinaryState.Low);

                _isOn = false;
                _stopwatch.Restart();
            }
        }
    }
}
