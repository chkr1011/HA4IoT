using System;
using System.Threading.Tasks;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Hardware.Services;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Controller.Main.Main.Rooms;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2C.I2CHardwareBridge;
using HA4IoT.Hardware.RemoteSwitch;
using HA4IoT.Hardware.RemoteSwitch.Adapters;
using HA4IoT.Hardware.RemoteSwitch.Codes.Protocols;
using HA4IoT.Hardware.Services;

namespace HA4IoT.Controller.Main.Main
{
    internal sealed class Configuration : IConfiguration
    {
        private readonly InterruptMonitorService _interruptMonitorService;
        private readonly CCToolsDeviceService _ccToolsBoardService;
        private readonly IGpioService _gpioService;
        private readonly IDeviceRegistryService _deviceService;
        private readonly II2CBusService _i2CBusService;
        private readonly ISchedulerService _schedulerService;
        private readonly RemoteSocketService _remoteSocketService;
        private readonly IContainer _containerService;

        public Configuration(
            CCToolsDeviceService ccToolsBoardService,
            IGpioService gpioService,
            IDeviceRegistryService deviceService,
            II2CBusService i2CBusService,
            ISchedulerService schedulerService,
            RemoteSocketService remoteSocketService,
            InterruptMonitorService interruptMonitorService,
            IContainer containerService)
        {
            _interruptMonitorService = interruptMonitorService ?? throw new ArgumentNullException(nameof(interruptMonitorService));
            _ccToolsBoardService = ccToolsBoardService ?? throw new ArgumentNullException(nameof(ccToolsBoardService));
            _gpioService = gpioService ?? throw new ArgumentNullException(nameof(gpioService));
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _i2CBusService = i2CBusService ?? throw new ArgumentNullException(nameof(i2CBusService));
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
            _remoteSocketService = remoteSocketService ?? throw new ArgumentNullException(nameof(remoteSocketService));
            _containerService = containerService ?? throw new ArgumentNullException(nameof(containerService));
        }

        public Task ApplyAsync()
        {
            _interruptMonitorService.RegisterInterrupt("Default", _gpioService.GetInput(4));
            _interruptMonitorService.RegisterCallback("Default", _ccToolsBoardService.PollInputBoardStates);

            _ccToolsBoardService.RegisterHSPE16InputOnly(InstalledDevice.Input0.ToString(), new I2CSlaveAddress(42));
            _ccToolsBoardService.RegisterHSPE16InputOnly(InstalledDevice.Input1.ToString(), new I2CSlaveAddress(43));
            _ccToolsBoardService.RegisterHSPE16InputOnly(InstalledDevice.Input2.ToString(), new I2CSlaveAddress(47));
            _ccToolsBoardService.RegisterHSPE16InputOnly(InstalledDevice.Input3.ToString(), new I2CSlaveAddress(45));
            _ccToolsBoardService.RegisterHSPE16InputOnly(InstalledDevice.Input4.ToString(), new I2CSlaveAddress(46));
            _ccToolsBoardService.RegisterHSPE16InputOnly(InstalledDevice.Input5.ToString(), new I2CSlaveAddress(44));

            var i2CHardwareBridge = new I2CHardwareBridge(new I2CSlaveAddress(50), _i2CBusService, _schedulerService);
            _deviceService.RegisterDevice(i2CHardwareBridge);

            _remoteSocketService.Adapter = new I2CHardwareBridgeLdp433MhzBridgeAdapter(i2CHardwareBridge, 10);
            var codeSequenceProvider = new DipswitchCodeProvider();
            _remoteSocketService.RegisterRemoteSocket("OFFICE_0", codeSequenceProvider.GetCodePair(DipswitchSystemCode.AllOn, DipswitchUnitCode.A));

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

            return Task.FromResult(0);
        }
    }
}
