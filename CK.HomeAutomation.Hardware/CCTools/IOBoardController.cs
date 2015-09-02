using System;
using System.Collections.Generic;
using System.Linq;
using CK.HomeAutomation.Core;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Hardware.CCTools
{
    public abstract class IOBoardController
    {
        private readonly object _syncRoot = new object();
        private readonly Dictionary<int, IOBoardPort> _openPorts = new Dictionary<int, IOBoardPort>();
        private readonly byte[] _state;
        private readonly byte[] _committedState;

        private readonly IDeviceDriver _portExpanderDriver;
        private readonly INotificationHandler _notificationHandler;

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

        public event EventHandler<IOBoardStateChangedEventArgs> StateChanged;

        public string Id { get; }

        public bool AutomaticallyFetchState { get; set; }

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
                // TODO: Consider add PeekState() which will read the state but will now fire events. This can be done using another method.
                // TODO: This will ensure that every state is read before time consuming actions (triggered by events) will be executed.

                byte[] currentState = _portExpanderDriver.Read();
                if (currentState.SequenceEqual(_committedState))
                {
                    return;
                }

                byte[] oldState = GetState();
                
                Array.Copy(currentState, _state, _state.Length);
                Array.Copy(currentState, _committedState, _committedState.Length);

                _notificationHandler.PublishFrom(this, NotificationType.Verbose, "'{0}' fetched different state.", Id);
                StateChanged?.Invoke(this, new IOBoardStateChangedEventArgs(oldState, currentState));
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