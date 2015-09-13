using System;
using System.Collections.Generic;
using System.Linq;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Hardware.GenericIOBoard
{
    public abstract class IOBoardController
    {
        private readonly byte[] _committedState;
        private readonly byte[] _state;

        private readonly INotificationHandler _notificationHandler;
        private readonly Dictionary<int, IOBoardPort> _openPorts = new Dictionary<int, IOBoardPort>();
        private readonly IDeviceDriver _portExpanderDriver;
        private readonly object _syncRoot = new object();

        protected IOBoardController(string id, IDeviceDriver portExpanderDriver, INotificationHandler notificationHandler)
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

                _notificationHandler.PublishFrom(this, NotificationType.Verbose, "'{0}' commited state.", Id);
            }
        }

        public void FetchState()
        {
            lock (_syncRoot)
            {
                // TODO: Consider add PeekState() which will read the state but will not fire events. This can be done using another method.
                // TODO: This will ensure that every state is read before time consuming actions (triggered by events) will be executed.

                byte[] newState = _portExpanderDriver.Read();
                if (newState.SequenceEqual(_committedState))
                {
                    return;
                }

                byte[] oldState = GetState();
                
                Array.Copy(newState, _state, _state.Length);
                Array.Copy(newState, _committedState, _committedState.Length);

                _notificationHandler.PublishFrom(this, NotificationType.Verbose, "'{0}' fetched different state (Old/New: {1}/{2}).", Id,
                    ByteExtensions.ToString(oldState), ByteExtensions.ToString(newState));

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