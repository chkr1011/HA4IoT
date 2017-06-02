using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.RemoteSockets;
using HA4IoT.Contracts.Hardware.RemoteSockets.Adapters;
using HA4IoT.Contracts.Hardware.RemoteSockets.Codes;
using HA4IoT.Contracts.Hardware.RemoteSockets.Configuration;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Scheduling;
using HA4IoT.Contracts.Services;
using HA4IoT.Hardware.Drivers.RemoteSockets;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Hardware.RemoteSockets
{
    [ApiServiceClass(typeof(IRemoteSocketService))]
    public sealed class RemoteSocketService : ServiceBase, IRemoteSocketService
    {
        private readonly ISystemInformationService _systemInformationService;
        private readonly IDeviceRegistryService _deviceRegistryService;
        private readonly object _syncRoot = new object();
        private readonly Dictionary<string, RemoteSocketOutputPort> _ports = new Dictionary<string, RemoteSocketOutputPort>();
        private readonly IConfigurationService _configurationService;
        private readonly ILogger _log;

        private Lpd433MhzCode _lastReceivedLpd433MhzCode;

        public RemoteSocketService(
            IConfigurationService configurationService,
            IDeviceRegistryService deviceRegistryService,
            ISchedulerService schedulerService,
            ISystemInformationService systemInformationService,
            ILogService logService)
        {

            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _deviceRegistryService = deviceRegistryService ?? throw new ArgumentNullException(nameof(deviceRegistryService));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            _systemInformationService = systemInformationService ?? throw new ArgumentNullException(nameof(systemInformationService));
            if (logService == null) throw new ArgumentNullException(nameof(logService));

            // Ensure that the state of the remote switch is restored if the original remote is used
            // or the switch has been removed from the socket and plugged in at another place.
            schedulerService.Register("RCSocketStateSender", TimeSpan.FromMinutes(1), () => RefreshStates());

            _log = logService.CreatePublisher(nameof(RemoteSocketService));
        }

        public void RegisterRemoteSockets()
        {
            var adapterDevices = _deviceRegistryService.GetDevices<ILdp433MhzBridgeAdapter>();
            foreach (var adapterDevice in adapterDevices)
            {
                adapterDevice.CodeReceived += OnCodeReceived;
            }

            var configuration = _configurationService.GetConfiguration<RemoteSocketServiceConfiguration>("RemoteSocketService");
            foreach (var remoteSocketConfiguration in configuration.RemoteSockets)
            {
                var codePair = GenerateCodePair(remoteSocketConfiguration.Value.CodeGenerator);
                RegisterRemoteSocket(remoteSocketConfiguration.Key, remoteSocketConfiguration.Value.Adapter.DeviceId, codePair);
            }
        }

        public IBinaryOutput RegisterRemoteSocket(string id, string adapterDeviceId, Lpd433MhzCodePair codePair)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (adapterDeviceId == null) throw new ArgumentNullException(nameof(adapterDeviceId));
            if (codePair == null) throw new ArgumentNullException(nameof(codePair));

            lock (_syncRoot)
            {
                var adapter = _deviceRegistryService.GetDevice<ILdp433MhzBridgeAdapter>(adapterDeviceId);
                var port = new RemoteSocketOutputPort(codePair, adapter);
                _ports.Add(id, port);
                port.Write(BinaryState.Low);

                _log.Info($"Registered remote socket '{id}'.");
                return port;
            }
        }

        public IBinaryOutput GetRemoteSocket(string id)
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

        public void SendCode(string adapterDeviceId, Lpd433MhzCode code)
        {
            if (adapterDeviceId == null) throw new ArgumentNullException(nameof(adapterDeviceId));
            if (code == null) throw new ArgumentNullException(nameof(code));

            var device = _deviceRegistryService.GetDevice<ILdp433MhzBridgeAdapter>(adapterDeviceId);
            device.SendCode(code);
        }

        [ApiMethod]
        public void Send(IApiCall apiCall)
        {
            var adapterDeviceId = apiCall.Parameter["AdapterDeviceId"].ToObject<string>();
            var code = apiCall.Parameter["Code"].ToObject<Lpd433MhzCode>();
            if (code == null)
            {
                apiCall.ResultCode = ApiResultCode.InvalidParameter;
                return;
            }

            SendCode(adapterDeviceId, code);
        }

        [ApiMethod]
        public void GetLastReceivedCode(IApiCall apiCall)
        {
            if (_lastReceivedLpd433MhzCode == null)
            {
                return;
            }

            lock (_syncRoot)
            {
                apiCall.Result = JObject.FromObject(_lastReceivedLpd433MhzCode);
            }
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
            _systemInformationService.Set($"{nameof(RemoteSocketService)}/LastReceivedCode", e.Code);

            lock (_syncRoot)
            {
                _lastReceivedLpd433MhzCode = e.Code;
            }
        }

        private Lpd433MhzCodePair GenerateCodePair(RemoteSocketCodeGeneratorConfiguration codeGeneratorConfiguration)
        {
            switch (codeGeneratorConfiguration.Type)
            {
                case "Dipswitch":
                    {
                        var dipswitchConfiguration = codeGeneratorConfiguration.Parameters.ToObject<DipswitchConfiguration>();
                        return DipswitchCodeProvider.GetCodePair(dipswitchConfiguration.SystemCode, dipswitchConfiguration.UnitCode);
                    }

                case "Intertechno":
                    {
                        var intertechnoConfiguration = codeGeneratorConfiguration.Parameters.ToObject<IntertechnoConfiguration>();
                        return IntertechnoCodeProvider.GetCodePair(intertechnoConfiguration.SystemCode, intertechnoConfiguration.UnitCode);
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }
    }
}
