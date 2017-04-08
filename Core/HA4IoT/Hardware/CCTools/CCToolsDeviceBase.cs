using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Hardware.CCTools
{
    public abstract class CCToolsDeviceBase : IDevice
    {
        private readonly object _syncRoot = new object();

        private readonly Dictionary<int, CCToolsDevicePort> _openPorts = new Dictionary<int, CCToolsDevicePort>();

        private readonly I2CIPortExpanderDriver _portExpanderDriver;
        private readonly IDeviceMessageBrokerService _deviceMessageBrokerService;
        private readonly ILogger _log;

        private readonly byte[] _committedState;
        private readonly byte[] _state;
        private byte[] _peekedState;

        protected CCToolsDeviceBase(string id, I2CIPortExpanderDriver portExpanderDriver, IDeviceMessageBrokerService deviceMessageBrokerService, ILogger log)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _portExpanderDriver = portExpanderDriver ?? throw new ArgumentNullException(nameof(portExpanderDriver));
            _deviceMessageBrokerService = deviceMessageBrokerService ?? throw new ArgumentNullException(nameof(deviceMessageBrokerService));

            _committedState = new byte[portExpanderDriver.StateSize];
            _state = new byte[portExpanderDriver.StateSize];
        }

        public event EventHandler<CCToolsDeviceStateChangedEventArgs> StateChanged;

        public string Id { get; }

        public byte[] GetState()
        {
            lock (_syncRoot)
            {
                return _state.ToArray();
            }
        }

        public byte[] GetCommittedState()
        {
            lock (_syncRoot)
            {
                return _committedState.ToArray();
            }
        }

        public void SetState(byte[] state)
        {
            lock (_syncRoot)
            {
                Array.Copy(state, _state, state.Length);
            }
        }

        protected CCToolsDevicePort GetPort(int number)
        {
            lock (_syncRoot)
            {
                CCToolsDevicePort port;
                if (!_openPorts.TryGetValue(number, out port))
                {
                    port = new CCToolsDevicePort(number, this);
                    _openPorts.Add(number, port);
                }

                return port;
            }
        }

        public void CommitChanges(bool force = false)
        {
            lock (_syncRoot)
            {
                if (!force && _state.SequenceEqual(_committedState))
                {
                    return;
                }

                _portExpanderDriver.Write(_state);
                Array.Copy(_state, _committedState, _state.Length);

                _log.Verbose(Id + ": Committed state");
            }
        }

        /// <summary>
        /// Reads the current state from the port expander but does not fire any events.
        /// Call <see cref="FetchState"/> after all port expanders are polled (peeked) to fire the events.
        /// </summary>
        public void PeekState()
        {
            lock (_syncRoot)
            {
                if (_peekedState != null)
                {
                    _log.Warning("Peeking state while previous peeked state is not processed at " + Id + "'.");
                }

                _peekedState = _portExpanderDriver.Read();
            }
        }

        /// <summary>
        /// Compares the peeked state and the previous state and fires events if the state has changed.
        /// This method calls method <see cref="PeekState"/> automatically if the state is not peeked.
        /// </summary>
        public void FetchState()
        {
            lock (_syncRoot)
            {
                if (_peekedState == null)
                {
                    PeekState();
                }

                var newState = _peekedState;
                _peekedState = null;

                if (newState.SequenceEqual(_committedState))
                {
                    return;
                }

                var oldState = GetState();

                Array.Copy(newState, _state, _state.Length);
                Array.Copy(newState, _committedState, _committedState.Length);

                var statesText = $"{BitConverter.ToString(oldState)},{BitConverter.ToString(newState)}";
                _log.Info($"'{Id}' fetched different state ({statesText})");

                _deviceMessageBrokerService.PublishDeviceMessage(Id, "StateChanged", statesText);
                StateChanged?.Invoke(this, new CCToolsDeviceStateChangedEventArgs(oldState, newState));
            }
        }

        internal BinaryState GetPortState(int pinNumber)
        {
            lock (_syncRoot)
            {
                return _state.GetBit(pinNumber) ? BinaryState.High : BinaryState.Low;
            }
        }

        internal void SetPortState(int pinNumber, BinaryState state)
        {
            lock (_syncRoot)
            {
                _state.SetBit(pinNumber, state == BinaryState.High);
            }
        }
    }
}
