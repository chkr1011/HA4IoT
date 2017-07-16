using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Hardware.Interrupts
{
    public sealed class InterruptMonitor
    {
        private readonly List<Action> _callbacks = new List<Action>();
        private readonly string _id;
        private readonly ILogger _log;

        public InterruptMonitor(string id, IBinaryInput input, ILogService logService)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            _id = id ?? throw new ArgumentNullException(nameof(id));
            _log = logService?.CreatePublisher(nameof(InterruptMonitor)) ?? throw new ArgumentNullException(nameof(logService));

            input.StateChanged += HandleInterrupt;
        }

        public void AddCallback(Action callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            lock (_callbacks)
            {
                _callbacks.Add(callback);
            }
        }

        private void HandleInterrupt(object sender, BinaryStateChangedEventArgs e)
        {
            Task.Run(() =>
            {
                List<Action> callbacks;
                lock (_callbacks)
                {
                    callbacks = new List<Action>(_callbacks);
                }

                foreach (var callback in callbacks)
                {
                    TryExecuteCallback(callback);
                }
            });

            _log.Info("Detected interrupt at monitor '" + _id + "'.");
        }

        private void TryExecuteCallback(Action callback)
        {
            try
            {
                callback();
            }
            catch (Exception exception)
            {
                _log.Error(exception, $"Failed to executing callback of interrupt '{_id}'.");
            }
        }
    }
}
