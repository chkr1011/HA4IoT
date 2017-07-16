using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Devices;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Hardware.Drivers.CCTools
{
    public abstract class CCToolsDeviceBase : IDevice
    {
        private readonly object _syncRoot = new object();
        private readonly Dictionary<int, CCToolsDevicePort> _openPorts = new Dictionary<int, CCToolsDevicePort>();
        private readonly II2CPortExpanderDriver _portExpanderDriver;
        private readonly ILogger _log;

        private readonly byte[] _committedState;
        private readonly byte[] _state;
        
        protected CCToolsDeviceBase(string id, II2CPortExpanderDriver portExpanderDriver, ILogger log)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            _portExpanderDriver = portExpanderDriver ?? throw new ArgumentNullException(nameof(portExpanderDriver));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            
            _state = new byte[portExpanderDriver.StateSize];
            _committedState = new byte[portExpanderDriver.StateSize];
        }

        public string Id { get; }

        public void FetchState()
        {
            lock (_syncRoot)
            {
                var newState = _portExpanderDriver.Read();
                if (newState.SequenceEqual(_state))
                {
                    return;
                }

                var oldState = _state.ToArray();

                Buffer.BlockCopy(newState, 0, _state, 0, newState.Length);
                Buffer.BlockCopy(newState, 0, _committedState, 0, newState.Length);

                var oldStateBits = new BitArray(oldState);
                var newStateBits = new BitArray(newState);
                foreach (var openPort in _openPorts)
                {
                    openPort.Value.OnBoardStateChanged(oldStateBits, newStateBits);
                }

                var statesText = BitConverter.ToString(oldState) + "->" + BitConverter.ToString(newState);
                _log.Info("'" + Id + "' fetched different state (" + statesText + ")");
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

        protected void CommitChanges(bool force = false)
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

        internal BinaryState GetPortState(int id)
        {
            lock (_syncRoot)
            {
                return _state.GetBit(id) ? BinaryState.High : BinaryState.Low;
            }
        }

        internal void SetPortState(int pinNumber, BinaryState state, bool commit)
        {
            lock (_syncRoot)
            {
                _state.SetBit(pinNumber, state == BinaryState.High);

                if (commit)
                {
                    CommitChanges();
                }
            }
        }
    }
}
