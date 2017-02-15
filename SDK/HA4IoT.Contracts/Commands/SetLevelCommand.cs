namespace HA4IoT.Contracts.Commands
{
    public class SetLevelCommand : ICommand
    {
        public SetLevelCommand(int level)
        {
            Level = level;
        }

        public int Level { get; }
    }
}
