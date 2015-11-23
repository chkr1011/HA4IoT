using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Notifications;

namespace HA4IoT.Hardware.GenericIOBoard
{
    public abstract class IOBoardController
    {
        private readonly byte[] _committedState;
        private readonly byte[] _state;
        private byte[] _peekedState;

        private readonly INotificationHandler _notificationHandler;
        private readonly Dictionary<int, IOBoardPort> _openPorts = new Dictionary<int, IOBoardPort>();
        private readonly IPortExpanderDriver _portExpanderDriver;
        private readonly object _syncRoot = new object();

        protected IOBoardController(string id, IPortExpanderDriver portExpanderDriver, INotificationHandler notificationHandler)
        {
            if (portExpanderDriver == null) throw new ArgumentNullException(nameof(portExpanderDriver));
            if (notificationHandler == null) throw new ArgumentNullException(nameof(notificationHandler));

            Id = id;

            _portExpanderDriver = portExpanderDriver;
            _notificationHandler = notificationHandler;

            _state = new byte[portExpanderDriver.StateSize];
            _committedState = new byte[portExpanderDriver.StateSize];
        }

        public string Id { get; }

        public bool AutomaticallyFetchState { get; set; }

        public event EventHandler<IOBoardStateChangedEventArgs> StateChanged;

        public byte[] GetState()
        {
            lock (_syncRoot)
            {
                return new List<byte>(_state).ToArray();
            }
        }

        public byte[] GetCommittedState()
        {
            lock (_syncRoot)
            {
                return new List<byte>(_committedState).ToArray();
            }
        }

        public void SetState(byte[] state)
        {
            lock (_syncRoot)
            {
                Array.Copy(state, _state, state.Length);
            }
        }

        protected IOBoardPort GetPort(int number)
        {
            lock (_syncRoot)
            {
                IOBoardPort port;
                if (!_openPorts.TryGetValue(number, out port))
                {
                    port = new IOBoardPort(number, this);
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

                _notificationHandler.Verbose(Id + ": Committed state");
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
                    _notificationHandler.Warning("Peeking state while previous peeked state is not processed at " + Id + "'.");
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

                byte[] newState = _peekedState;
                _peekedState = null;

                if (newState.SequenceEqual(_committedState))
                {
                    return;
                }

                byte[] oldState = GetState();
                
                Array.Copy(newState, _state, _state.Length);
                Array.Copy(newState, _committedState, _committedState.Length);

                _notificationHandler.Verbose("'" + Id + "' fetched different state (" +
                                             ByteExtensions.ToString(oldState) + "->" +
                                             ByteExtensions.ToString(newState) + ").");

                StateChanged?.Invoke(this, new IOBoardStateChangedEventArgs(oldState, newState));
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