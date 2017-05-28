using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Devices;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Hardware.CCTools
{
    public abstract class CCToolsDeviceBase : IDevice
    {
        private readonly object _syncRoot = new object();
        private readonly Dictionary<int, CCToolsDevicePort> _openPorts = new Dictionary<int, CCToolsDevicePort>();
        private readonly I2CIPortExpanderDriver _portExpanderDriver;
        private readonly ILogger _log;

        private readonly byte[] _committedState;
        private readonly byte[] _state;

        private byte[] _peekedState;

        protected CCToolsDeviceBase(string id, I2CIPortExpanderDriver portExpanderDriver, ILogger log)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _portExpanderDriver = portExpanderDriver ?? throw new ArgumentNullException(nameof(portExpanderDriver));
            
            _state = new byte[portExpanderDriver.StateSize];
            _committedState = new byte[portExpanderDriver.StateSize];
        }

        public string Id { get; }

        protected byte[] GetState()
        {
            lock (_syncRoot)
            {
                return _state.ToArray();
            }
        }

        protected void SetState(byte[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            lock (_syncRoot)
            {
                Buffer.BlockCopy(state, 0, _state, 0, state.Length);
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
                Buffer.BlockCopy(_state, 0,  _committedState, 0, _state.Length);

                _log.Verbose("Board '" + Id + "' committed state '" + BitConverter.ToString(_state) + "'.");
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
                    _peekedState = _portExpanderDriver.Read();
                }

                var newState = _peekedState;
                _peekedState = null;

                if (DataHasChanged(_state, newState))
                {
                    return;
                }

                var oldState = GetState();
                
                Buffer.BlockCopy(newState, 0, _state, 0, newState.Length);
                Buffer.BlockCopy(newState, 0, _committedState, 0, newState.Length);
                
                var oldStateBits = new BitArray(oldState);
                var newStateBits = new BitArray(newState);
                foreach (var openPort in _openPorts)
                {
                    openPort.Value.OnBoardStateChanged(oldStateBits, newStateBits);
                }

                var statesText = BitConverter.ToString(oldState) + "," + BitConverter.ToString(newState);
                _log.Info("'" + Id + "' fetched different state (" + statesText + ")");
            }
        }

        internal BinaryState GetPortState(int id)
        {
            lock (_syncRoot)
            {
                return _state.GetBit(id) ? BinaryState.High : BinaryState.Low;
            }
        }

        internal void SetPortState(int pinNumber, BinaryState state)
        {
            lock (_syncRoot)
            {
                _state.SetBit(pinNumber, state == BinaryState.High);
            }
        }

        private static bool DataHasChanged(byte[] data1, byte[] data2)
        {
            if (data1.Length != data2.Length)
            {
                return true;
            }

            for (var i = 0; i < data1.Length; i++)
            {
                if (data1[i] != data2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
