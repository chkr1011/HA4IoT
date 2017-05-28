namespace HA4IoT.Contracts.Health.Configuration
{
   public class StatusLedConfiguration
    {
        public int? Gpio { get; set; }

        public bool IsInverted { get; set; } = true;
    }
}
