using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Hardware.Services
{
    public class InterruptMonitor
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
            lock (_callbacks)
            {
                _callbacks.Add(callback);
            }
        }

        private void HandleInterrupt(object sender, BinaryStateChangedEventArgs e)
        {
            Task.Run(() => HandleInterrupt());
        }

        private void HandleInterrupt()
        {
            List<Action> actions;
            lock (_callbacks)
            {
                actions = new List<Action>(_callbacks);
            }

            try
            {
                _log.Info("Detected interrupt at monitor '" + _id + "'.");

                foreach (var action in actions)
                {
                    action();
                }
            }
            catch (Exception exception)
            {
                _log.Error(exception, $"Error while executing callbacks of interrupt '{_id}'.");
            }
        }
    }
}
