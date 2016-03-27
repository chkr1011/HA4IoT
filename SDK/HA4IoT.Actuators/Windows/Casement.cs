using System;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Actuators
{
    public class Casement : ICasement
    {
        public const string LeftCasementId = "Left";
        public const string CenterCasementId = "Center";
        public const string RightCasementId = "Right";

        // TODO: Implement endpoint
        private readonly IBinaryInput _fullOpenReedSwitch;
        private readonly IBinaryInput _tiltReedSwitch;

        private readonly Trigger _openedTrigger = new Trigger();
        private readonly Trigger _closedTrigger = new Trigger();

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

        public event EventHandler<CasementStateChangedEventArgs> StateChanged;

        public CasementState GetState()
        {
            return _state;
        }

        private ITrigger GetOpenedTrigger()
        {
            return _openedTrigger;
        }

        public ITrigger GetClosedTrigger()
        {
            return _closedTrigger;
        }

        private void Update()
        {
            var oldState = _state;

            if (_fullOpenReedSwitch.Read() == BinaryState.Low)
            {
                _state = CasementState.Open;
                _openedTrigger.Execute();
                return;
            }

            if (_tiltReedSwitch != null && _tiltReedSwitch.Read() == BinaryState.Low)
            {
                _state = CasementState.Tilt;
                _closedTrigger.Execute();
                return;
            }
            else
            {
                _state = CasementState.Closed;
                _closedTrigger.Execute();
            }
            
            OnStateChanged(oldState, _state);
        }

        private void OnStateChanged(CasementState oldState, CasementState newState)
        {
            StateChanged?.Invoke(this, new CasementStateChangedEventArgs(oldState, newState));
        }
    }
}
