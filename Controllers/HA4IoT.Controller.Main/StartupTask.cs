using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
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
            var controller = new Core.Controller(new ControllerOptions { StatusLedNumber = LedGpio, Configuration = new MainConfiguration() });
            controller.RunAsync(taskInstance);
        }

        private class MainConfiguration : IConfiguration
        {
            public void SetupContainer(IContainerService containerService)
            {
            }

            public Task Configure(IContainerService containerService)
            {
                var ccToolsBoardService = containerService.GetInstance<CCToolsBoardService>();
                var pi2GpioService = containerService.GetInstance<IPi2GpioService>();
                var synonymService = containerService.GetInstance<SynonymService>();
                var deviceService = containerService.GetInstance<IDeviceService>();
                var i2CBusService = containerService.GetInstance<II2CBusService>();
                var schedulerService = containerService.GetInstance<ISchedulerService>();
                var remoteSocketService = containerService.GetInstance<RemoteSocketService>();
                var apiService = containerService.GetInstance<IApiService>();

                synonymService.TryLoadPersistedSynonyms();

                ccToolsBoardService.RegisterHSPE16InputOnly(InstalledDevice.Input0, new I2CSlaveAddress(42));
                ccToolsBoardService.RegisterHSPE16InputOnly(InstalledDevice.Input1, new I2CSlaveAddress(43));
                ccToolsBoardService.RegisterHSPE16InputOnly(InstalledDevice.Input2, new I2CSlaveAddress(47));
                ccToolsBoardService.RegisterHSPE16InputOnly(InstalledDevice.Input3, new I2CSlaveAddress(45));
                ccToolsBoardService.RegisterHSPE16InputOnly(InstalledDevice.Input4, new I2CSlaveAddress(46));
                ccToolsBoardService.RegisterHSPE16InputOnly(InstalledDevice.Input5, new I2CSlaveAddress(44));

                var i2CHardwareBridge = new I2CHardwareBridge(new I2CSlaveAddress(50), i2CBusService, schedulerService);
                deviceService.AddDevice(i2CHardwareBridge);

                remoteSocketService.Sender = new LPD433MHzSignalSender(i2CHardwareBridge, LDP433MhzSenderPin, apiService);
                var brennenstuhl = new BrennenstuhlCodeSequenceProvider();
                remoteSocketService.RegisterRemoteSocket(0, brennenstuhl.GetSequencePair(BrennenstuhlSystemCode.AllOn, BrennenstuhlUnitCode.A));

                containerService.GetInstance<BedroomConfiguration>().Apply();
                containerService.GetInstance<OfficeConfiguration>().Apply();
                containerService.GetInstance<UpperBathroomConfiguration>().Apply();
                containerService.GetInstance<ReadingRoomConfiguration>().Apply();
                containerService.GetInstance<ChildrensRoomRoomConfiguration>().Apply();
                containerService.GetInstance<KitchenConfiguration>().Apply();
                containerService.GetInstance<FloorConfiguration>().Apply();
                containerService.GetInstance<LowerBathroomConfiguration>().Apply();
                containerService.GetInstance<StoreroomConfiguration>().Apply();
                containerService.GetInstance<LivingRoomConfiguration>().Apply();

                synonymService.RegisterDefaultComponentStateSynonyms();

                var ioBoardsInterruptMonitor = new InterruptMonitor(pi2GpioService.GetInput(4));
                ioBoardsInterruptMonitor.InterruptDetected += (s, e) => ccToolsBoardService.PollInputBoardStates();
                ioBoardsInterruptMonitor.Start();

                return Task.FromResult(0);
            }
        }
    }
}