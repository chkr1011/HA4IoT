using System;
using System.Threading;
using System.Threading.Tasks;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Scheduling;

namespace HA4IoT.Scheduling
{
    public sealed class ScheduledAction : IScheduledAction, IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Action _action;
        private readonly TimeSpan _dueTime;

        private ScheduledAction(TimeSpan dueTime, Action action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _dueTime = dueTime;
        }

        public static IScheduledAction Schedule(TimeSpan dueTime, Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            var delayedAction = new ScheduledAction(dueTime, action);
            delayedAction.Schedule();

            return delayedAction;
        }

        public void Schedule()
        {
            Task.Run(WaitAndExecuteAsync);
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel(false);
        }

        public void Dispose()
        {
            Cancel();
        }

        private async Task WaitAndExecuteAsync()
        {
            try
            {
                await Task.Delay(_dueTime, _cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
            }

            if (_cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            try
            {
                _action();
            }
            catch (Exception exception)
            {
                Log.Default.Error(exception, "Error while executing scheduled action.");
            }
        }
    }
}
