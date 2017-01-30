namespace HA4IoT.FeatureRebuild.Commands
{
    public class SetColorCommand : ICommand
    {
        public byte Red { get; set; }

        public byte Green { get; set; }

        public byte Blue { get; set; }
    }
}
