using System;
using System.Threading;
using System.Threading.Tasks;
using HA4IoT.Contracts.Scheduling;

namespace HA4IoT.Scheduling
{
    public sealed class DelayedAction : IDelayedAction, IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Action _action;
        private readonly TimeSpan _delay;
        
        public DelayedAction(TimeSpan delay, Action action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _delay = delay;

            Task.Run(WaitAndExecuteAsync);
        }

        private async Task WaitAndExecuteAsync()
        {
            try
            {
                await Task.Delay(_delay, _cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
            }

            if (_cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            _action();
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel(false);
        }

        public void Dispose()
        {
            Cancel();
        }
    }
}
