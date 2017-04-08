namespace HA4IoT.Contracts.Commands
{
    public class SetLevelCommand : ICommand
    {
        public int Level { get; set; }
    }
}
