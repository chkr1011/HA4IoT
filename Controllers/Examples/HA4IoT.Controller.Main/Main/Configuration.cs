using System;
using System.Threading.Tasks;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Core;
using HA4IoT.Controller.Main.Main.Rooms;

namespace HA4IoT.Controller.Main.Main
{
    internal sealed class Configuration : IConfiguration
    {
        ////private readonly InterruptMonitorService _interruptMonitorService;
        ////private readonly CCToolsDeviceService _ccToolsBoardService;
        ////private readonly IGpioService _gpioService;
        ////private readonly IDeviceRegistryService _deviceService;
        ////private readonly II2CBusService _i2CBusService;
        ////private readonly ISchedulerService _schedulerService;
        ////private readonly IRemoteSocketService _remoteSocketService;
        private readonly IContainer _containerService;
        ////private readonly OutpostDeviceService _outpostDeviceService;

        public Configuration(IContainer containerService)
        {
            ////_interruptMonitorService = interruptMonitorService ?? throw new ArgumentNullException(nameof(interruptMonitorService));
            ////_ccToolsBoardService = ccToolsBoardService ?? throw new ArgumentNullException(nameof(ccToolsBoardService));
            ////_gpioService = gpioService ?? throw new ArgumentNullException(nameof(gpioService));
            ////_deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            ////_i2CBusService = i2CBusService ?? throw new ArgumentNullException(nameof(i2CBusService));
            ////_schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
            ////_remoteSocketService = remoteSocketService ?? throw new ArgumentNullException(nameof(remoteSocketService));
            _containerService = containerService ?? throw new ArgumentNullException(nameof(containerService));
            ////_outpostDeviceService = outpostDeviceService ?? throw new ArgumentNullException(nameof(outpostDeviceService));
        }

        public Task ApplyAsync()
        {
            ////_interruptMonitorService.RegisterInterrupt("Default", _gpioService.GetInput(4).WithInvertedState());
            ////_interruptMonitorService.RegisterCallback("Default", _ccToolsBoardService.PollAllInputDevices);

            ////_ccToolsBoardService.RegisterDevice(CCToolsDeviceType.HSPE16_InputOnly, InstalledDevice.Input0.ToString(), 42);
            ////_ccToolsBoardService.RegisterDevice(CCToolsDeviceType.HSPE16_InputOnly, InstalledDevice.Input1.ToString(), 43);
            ////_ccToolsBoardService.RegisterDevice(CCToolsDeviceType.HSPE16_InputOnly, InstalledDevice.Input2.ToString(), 47);
            ////_ccToolsBoardService.RegisterDevice(CCToolsDeviceType.HSPE16_InputOnly, InstalledDevice.Input3.ToString(), 45);
            ////_ccToolsBoardService.RegisterDevice(CCToolsDeviceType.HSPE16_InputOnly, InstalledDevice.Input4.ToString(), 46);
            ////_ccToolsBoardService.RegisterDevice(CCToolsDeviceType.HSPE16_InputOnly, InstalledDevice.Input5.ToString(), 44);

            ////var i2CHardwareBridge = new I2CHardwareBridge("I2CHardwareBridge", new I2CSlaveAddress(50), _i2CBusService, _schedulerService);
            ////_deviceService.RegisterDevice(i2CHardwareBridge);

            //_deviceService.RegisterDevice(_outpostDeviceService.GetLpdBridgeAdapter("LPDB1", "LPDB1"));

            //var codeSequenceProvider = new DipswitchCodeProvider();
            //_remoteSocketService.RegisterRemoteSocket("OFFICE_0", "LPDB1", codeSequenceProvider.GetCodePair(DipswitchSystemCode.AllOn, DipswitchUnitCode.A));

            _containerService.GetInstance<OfficeConfiguration>().Apply();
            _containerService.GetInstance<BedroomConfiguration>().Apply();
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
