using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Actuators.Animations
{
    public class Animation
    {
        private readonly ITimerService _timerService;

        private TimeSpan _position = TimeSpan.Zero;

        public Animation(ITimerService timerService)
        {
            if (timerService == null) throw new ArgumentNullException(nameof(timerService));

            _timerService = timerService;
        }

        public IList<Frame> Frames { get; } = new List<Frame>();

        public virtual void Start()
        {
            ApplyFrame();
            _timerService.Tick += ApplyFrame;
        }

        public void Stop()
        {
            _timerService.Tick -= ApplyFrame;
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
