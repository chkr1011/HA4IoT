namespace HA4IoT.Contracts.Components.Commands
{
    public class SetLevelCommand : ICommand
    {
        public int Level { get; set; }
    }
}
