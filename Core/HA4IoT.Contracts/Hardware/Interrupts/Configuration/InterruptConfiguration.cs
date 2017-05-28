using HA4IoT.Contracts.Hardware.RaspberryPi;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HA4IoT.Contracts.Hardware.Interrupts.Configuration
{
    public class InterruptConfiguration
    {
        public int Gpio { get; set; }

        public bool IsInverted { get; set; } = true;

        [JsonConverter(typeof(StringEnumConverter))]
        public GpioInputMonitoringMode MonitoringMode { get; set; } = GpioInputMonitoringMode.Interrupt;

        [JsonConverter(typeof(StringEnumConverter))]
        public GpioPullMode PullMode { get; set; } = GpioPullMode.None;
    }
}
