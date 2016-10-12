using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Controller.Main.Rooms;
using HA4IoT.Core;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Hardware.RemoteSwitch;
using HA4IoT.Hardware.RemoteSwitch.Codes;
using HA4IoT.PersonalAgent;

namespace HA4IoT.Controller.Main
{
    public sealed class StartupTask : IBackgroundTask
    {
        private const int LedGpio = 22;
        private const int LDP433MhzSenderPin = 10;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var options = new ControllerOptions
            {
                StatusLedNumber = LedGpio,
                ConfigurationType = typeof(Configuration)
            };

            var controller = new Core.Controller(options);
            controller.RunAsync(taskInstance);
        }

        private class Configuration : IConfiguration
        {
            private readonly CCToolsBoardService _ccToolsBoardService;
            private readonly IPi2GpioService _pi2GpioService;
            private readonly SynonymService _synonymService;
            private readonly IDeviceService _deviceService;
            private readonly II2CBusService _i2CBusService;
            private readonly ISchedulerService _schedulerService;
            private readonly RemoteSocketService _remoteSocketService;
            private readonly IApiService _apiService;
            private readonly IContainer _containerService;

            public Configuration(
                CCToolsBoardService ccToolsBoardService, 
                IPi2GpioService pi2GpioService, 
                SynonymService synonymService,
                IDeviceService deviceService,
                II2CBusService i2CBusService, 
                ISchedulerService schedulerService, 
                RemoteSocketService remoteSocketService, 
                IApiService apiService,
                IContainer containerService)
            {
                if (ccToolsBoardService == null) throw new ArgumentNullException(nameof(ccToolsBoardService));
                if (pi2GpioService == null) throw new ArgumentNullException(nameof(pi2GpioService));
                if (synonymService == null) throw new ArgumentNullException(nameof(synonymService));
                if (deviceService == null) throw new ArgumentNullException(nameof(deviceService));
                if (i2CBusService == null) throw new ArgumentNullException(nameof(i2CBusService));
                if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
                if (remoteSocketService == null) throw new ArgumentNullException(nameof(remoteSocketService));
                if (apiService == null) throw new ArgumentNullException(nameof(apiService));
                if (containerService == null) throw new ArgumentNullException(nameof(containerService));

                _ccToolsBoardService = ccToolsBoardService;
                _pi2GpioService = pi2GpioService;
                _synonymService = synonymService;
                _deviceService = deviceService;
                _i2CBusService = i2CBusService;
                _schedulerService = schedulerService;
                _remoteSocketService = remoteSocketService;
                _apiService = apiService;
                _containerService = containerService;
            }

            public Task ApplyAsync()
            {
                _synonymService.TryLoadPersistedSynonyms();

                _ccToolsBoardService.RegisterHSPE16InputOnly(InstalledDevice.Input0, new I2CSlaveAddress(42));
                _ccToolsBoardService.RegisterHSPE16InputOnly(InstalledDevice.Input1, new I2CSlaveAddress(43));
                _ccToolsBoardService.RegisterHSPE16InputOnly(InstalledDevice.Input2, new I2CSlaveAddress(47));
                _ccToolsBoardService.RegisterHSPE16InputOnly(InstalledDevice.Input3, new I2CSlaveAddress(45));
                _ccToolsBoardService.RegisterHSPE16InputOnly(InstalledDevice.Input4, new I2CSlaveAddress(46));
                _ccToolsBoardService.RegisterHSPE16InputOnly(InstalledDevice.Input5, new I2CSlaveAddress(44));

                var i2CHardwareBridge = new I2CHardwareBridge(new I2CSlaveAddress(50), _i2CBusService, _schedulerService);
                _deviceService.AddDevice(i2CHardwareBridge);

                _remoteSocketService.Sender = new LPD433MHzSignalSender(i2CHardwareBridge, LDP433MhzSenderPin, _apiService);
                var brennenstuhl = new BrennenstuhlCodeSequenceProvider();
                _remoteSocketService.RegisterRemoteSocket(0, brennenstuhl.GetSequencePair(BrennenstuhlSystemCode.AllOn, BrennenstuhlUnitCode.A));

                _containerService.GetInstance<BedroomConfiguration>().Apply();
                _containerService.GetInstance<OfficeConfiguration>().Apply();
                _containerService.GetInstance<UpperBathroomConfiguration>().Apply();
                _containerService.GetInstance<ReadingRoomConfiguration>().Apply();
                _containerService.GetInstance<ChildrensRoomRoomConfiguration>().Apply();
                _containerService.GetInstance<KitchenConfiguration>().Apply();
                _containerService.GetInstance<FloorConfiguration>().Apply();
                _containerService.GetInstance<LowerBathroomConfiguration>().Apply();
                _containerService.GetInstance<StoreroomConfiguration>().Apply();
                _containerService.GetInstance<LivingRoomConfiguration>().Apply();

                _synonymService.RegisterDefaultComponentStateSynonyms();

                var ioBoardsInterruptMonitor = new InterruptMonitor(_pi2GpioService.GetInput(4));
                ioBoardsInterruptMonitor.InterruptDetected += (s, e) => _ccToolsBoardService.PollInputBoardStates();
                ioBoardsInterruptMonitor.Start();

                return Task.FromResult(0);
            }
        }
    }
}