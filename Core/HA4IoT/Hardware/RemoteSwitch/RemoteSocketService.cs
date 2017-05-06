using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.RemoteSwitch.Codes;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Hardware.RemoteSwitch
{
    [ApiServiceClass(typeof(RemoteSocketService))]
    public class RemoteSocketService : ServiceBase
    {
        private readonly object _syncRoot = new object();
        private readonly Dictionary<string, RemoteSocketOutputPort> _ports = new Dictionary<string, RemoteSocketOutputPort>();
        private readonly ILogger _log;

        private ILdp433MhzBridgeAdapter _adapter;
        private Lpd433MhzCode _lastReceivedLpd433MhzCode;

        public RemoteSocketService(ISchedulerService schedulerService, ILogService logService)
        {
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            if (logService == null) throw new ArgumentNullException(nameof(logService));

            // Ensure that the state of the remote switch is restored if the original remote is used
            // or the switch has been removed from the socket and plugged in at another place.
            schedulerService.RegisterSchedule("RCSocketStateSender", TimeSpan.FromMinutes(1), () => RefreshStates());

            _log = logService.CreatePublisher(nameof(RemoteSocketService));
        }

        public ILdp433MhzBridgeAdapter Adapter
        {
            get => _adapter;
            set
            {
                if (_adapter != null)
                {
                    throw new InvalidOperationException("The Lpd433MhzAdapter can be set only once.");
                }

                _adapter = value;
                _adapter.CodeReceived += OnCodeReceived;
            }
        }

        [ApiMethod]
        public void Send(IApiContext apiContext)
        {
            var code = apiContext.Parameter.ToObject<Lpd433MhzCode>();
            if (code == null)
            {
                apiContext.ResultCode = ApiResultCode.InvalidParameter;
                return;
            }

            SendCode(code);
        }

        [ApiMethod]
        public void GetLastReceivedCode(IApiContext apiContext)
        {
            if (_lastReceivedLpd433MhzCode == null)
            {
                return;
            }

            lock (_syncRoot)
            {
                apiContext.Result = JObject.FromObject(_lastReceivedLpd433MhzCode);
            }
        }

        public RemoteSocketOutputPort RegisterRemoteSocket(string id, Lpd433MhzCodePair codePair)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (codePair == null) throw new ArgumentNullException(nameof(codePair));

            lock (_syncRoot)
            {
                var port = new RemoteSocketOutputPort(codePair, this);
                _ports.Add(id, port);
                port.Write(BinaryState.Low);

                _log.Info($"Registered remote socket '{id}'.");
                return port;
            }
        }

        public RemoteSocketOutputPort GetRemoteSocket(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            
            lock (_syncRoot)
            {
                RemoteSocketOutputPort output;
                if (!_ports.TryGetValue(id, out output))
                {
                    throw new InvalidOperationException($"No remote socket with ID '{id}' registered.");    
                }

                return output;
            }
        }

        public void SendCode(Lpd433MhzCode code)
        {
            if (code == null) throw new ArgumentNullException(nameof(code));
            if (Adapter == null)
            {
                _log.Warning("Cannot send LPD code because adapter is not set.");
                return;
            }

            Adapter.SendCode(code);
        }

        private void RefreshStates()
        {
            foreach (var port in _ports.Values)
            {
                port.Write(port.Read());
            }
        }

        private void OnCodeReceived(object sender, Ldp433MhzCodeReceivedEventArgs e)
        {
            lock (_syncRoot)
            {
                _lastReceivedLpd433MhzCode = e.Code;
            }
        }
    }
}
