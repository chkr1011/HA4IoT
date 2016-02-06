using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Core;
using HA4IoT.Core.Timer;

namespace HA4IoT.Actuators.Animations
{
    public class Animation
    {
        private readonly IHomeAutomationTimer _timer;

        private TimeSpan _position = TimeSpan.Zero;

        public Animation(IHomeAutomationTimer timer)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            _timer = timer;
        }

        public IList<Frame> Frames { get; } = new List<Frame>();

        public virtual void Start()
        {
            ApplyFrame();
            _timer.Tick += ApplyFrame;
        }

        public void Stop()
        {
            _timer.Tick -= ApplyFrame;
        }

        public Animation WithFrame(Frame frame)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));

            Frames.Add(frame);
            return this;
        }

        private void ApplyFrame(object sender, TimerTickEventArgs timerTickEventArgs)
        {
            _position += timerTickEventArgs.ElapsedTime;
            ApplyFrame();
        }

        private void ApplyFrame()
        {
            foreach (var frame in Frames)
            {
                if (frame.IsApplied || frame.StartTime > _position)
                {
                    continue;
                }

                frame.Apply();
            }

            if (Frames.Last().IsApplied)
            {
                Stop();
            }
        }
    }
}
