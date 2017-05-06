namespace HA4IoT.Contracts.Logging
{
    public interface ILogAdapter
    {
        void ProcessLogEntry(LogEntry logEntry);
    }
}
