using System;

namespace HA4IoT.Contracts.Hardware.Interrupts
{
    public interface IInterruptMonitorService
    {
        void RegisterInterrupts();
        void RegisterCallback(string interruptMonitorId, Action callback);
    }
}