using System;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Hardware;

namespace HA4IoT.Actuators
{
    public class Casement
    {
        public const string LeftCasementId = "Left";
        public const string CenterCasementId = "Center";
        public const string RightCasementId = "Right";

        private readonly IBinaryInput _fullOpenReedSwitch;
        private readonly IBinaryInput _tiltReedSwitch;

        private CasementState _state = CasementState.Closed;

        public Casement(string id, IBinaryInput fullOpenReedSwitch, IBinaryInput tiltReedSwitch = null)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (fullOpenReedSwitch == null) throw new ArgumentNullException(nameof(fullOpenReedSwitch));

            Id = id;
            _fullOpenReedSwitch = fullOpenReedSwitch;
            _tiltReedSwitch = tiltReedSwitch;

            if (_tiltReedSwitch != null)
            {
                _tiltReedSwitch.StateChanged += (s, e) => Update();
            }

            _fullOpenReedSwitch.StateChanged += (s, e) => Update();

            Update();
        }

        public string Id { get; private set; }

        public CasementState State
        {
            get
            {
                return _state;
            }

            private set
            {
                _state = value;
                StateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler StateChanged;

        private void Update()
        {
            if (_fullOpenReedSwitch.Read() == BinaryState.Low)
            {
                State = CasementState.Open;
                return;
            }

            if (_tiltReedSwitch != null && _tiltReedSwitch.Read() == BinaryState.Low)
            {
                State = CasementState.Tilt;
                return;
            }

            State = CasementState.Closed;
        }
    }
}
