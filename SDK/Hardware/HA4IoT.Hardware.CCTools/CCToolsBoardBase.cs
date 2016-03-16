using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Networking;

namespace HA4IoT.Hardware.CCTools
{
    public abstract class CCToolsBoardBase : IDevice
    {
        private readonly object _syncRoot = new object();

        private readonly Dictionary<int, IOBoardPort> _openPorts = new Dictionary<int, IOBoardPort>();

        private readonly IApiController _apiController;
        private readonly IPortExpanderDriver _portExpanderDriver;
        
        private readonly byte[] _committedState;
        private readonly byte[] _state;
        private byte[] _peekedState;

        protected CCToolsBoardBase(DeviceId id, IPortExpanderDriver portExpanderDriver, IApiController apiController)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (portExpanderDriver == null) throw new ArgumentNullException(nameof(portExpanderDriver));
            if (apiController == null) throw new ArgumentNullException(nameof(apiController));

            Id = id;
            _portExpanderDriver = portExpanderDriver;

            _committedState = new byte[portExpanderDriver.StateSize];
            _state = new byte[portExpanderDriver.StateSize];

            _apiController = apiController;

            ExposeToApi();
        }

        public event EventHandler<IOBoardStateChangedEventArgs> StateChanged;
        public DeviceId Id { get; }

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

                Log.Verbose(Id + ": Committed state");
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
                    Log.Warning("Peeking state while previous peeked state is not processed at " + Id + "'.");
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

                Log.Verbose("'" + Id + "' fetched different state (" +
                                             ByteExtensions.ToString(oldState) + "->" +
                                             ByteExtensions.ToString(newState) + ").");

                StateChanged?.Invoke(this, new IOBoardStateChangedEventArgs(oldState, newState));
            }
        }

        private void ExposeToApi()
        {
            _apiController.RouteRequest($"device/{Id}", HandleApiGet);
            _apiController.RouteCommand($"device/{Id}", HandleApiPost);
        }

        private void HandleApiGet(IApiContext apiContext)
        {
            var result = new JsonObject();
            result.SetNamedValue("state", GetState().ToJsonValue());
            result.SetNamedValue("committed-state", GetCommittedState().ToJsonValue());

            apiContext.Response = result;
        }

        private void HandleApiPost(IApiContext apiContext)
        {
            JsonArray state = apiContext.Request.GetNamedArray("state", null);
            if (state != null)
            {
                byte[] buffer = JsonValueToByteArray(state);
                SetState(buffer);
            }

            var commit = apiContext.Request.GetNamedBoolean("commit", true);
            if (commit)
            {
                CommitChanges();
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

        private byte[] JsonValueToByteArray(JsonArray value)
        {
            return value.Select(item => (byte)item.GetNumber()).ToArray();
        }
    }
}
