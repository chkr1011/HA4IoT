using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Core.Timer;

namespace HA4IoT.Actuators.Animations
{
    public class Animation
    {
        public IList<Frame> Frames { get; } = new List<Frame>();
        private readonly IHomeAutomationTimer _timer;

        private TimeSpan _position = TimeSpan.Zero;

        public Animation(IHomeAutomationTimer timer)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            _timer = timer;
        }

        public virtual void Start()
        {
            UpdateFrame();
            _timer.Tick += OnTick;
        }

        public void Stop()
        {
            _timer.Tick -= OnTick;
        }

        public Animation WithFrame(Frame frame)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));

            Frames.Add(frame);
            return this;
        }

        private void UpdateFrame()
        {
            if (_position > Frames.Last().StartTime)
            {
                Stop();
                return;
            }

            Frame currentFrame = Frames.FirstOrDefault(f => _position >= f.StartTime);
            currentFrame.Apply();
        }

        private void OnTick(object sender, TimerTickEventArgs timerTickEventArgs)
        {
            _position += timerTickEventArgs.ElapsedTime;
            UpdateFrame();
        }
    }
}
