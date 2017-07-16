using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using HA4IoT.Contracts.Components.Adapters;

namespace HA4IoT.Simulator.Controls
{
    public class UIMotionDetectorAdapter : IMotionDetectorAdapter
    {
        private readonly CheckBox _checkBox;

        public UIMotionDetectorAdapter(CheckBox checkBox)
        {
            _checkBox = checkBox ?? throw new ArgumentNullException(nameof(checkBox));
        }

        public event EventHandler<MotionDetectorAdapterStateChangedEventArgs> StateChanged;

        public void Refresh()
        {
        }

        public void Connect()
        {
            _checkBox.Checked += (s, e) => OnMotionDetectionBegin();
            _checkBox.Unchecked += (s, e) => OnMotionDetectionEnd();
        }

        private void OnMotionDetectionBegin()
        {
            Task.Run(() => StateChanged?.Invoke(this, new MotionDetectorAdapterStateChangedEventArgs(AdapterMotionDetectionState.MotionDetected)));
        }

        private void OnMotionDetectionEnd()
        {
            Task.Run(() => StateChanged?.Invoke(this, new MotionDetectorAdapterStateChangedEventArgs(AdapterMotionDetectionState.Idle)));
        }
    }
}
