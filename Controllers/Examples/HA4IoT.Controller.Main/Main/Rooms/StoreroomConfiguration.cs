using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Areas;
using HA4IoT.Automations;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.ExternalServices.Twitter;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.CCTools.Devices;
using HA4IoT.Sensors;
using HA4IoT.Sensors.MotionDetectors;

namespace HA4IoT.Controller.Main.Main.Rooms
{
    internal class StoreroomConfiguration
    {
        private readonly IAreaRegistryService _areaService;
        private readonly IDeviceRegistryService _deviceService;
        private readonly CCToolsDeviceService _ccToolsBoardService;
        private readonly ITimerService _timerService;
        private readonly ITwitterClientService _twitterClientService;
        private readonly AutomationFactory _automationFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly SensorFactory _sensorFactory;
        private readonly ILogService _logService;
        private CatLitterBoxTwitterSender _catLitterBoxTwitterSender;

        private enum Storeroom
        {
            MotionDetector,
            MotionDetectorCatLitterBox,
            LightCeiling,
            LightCeilingAutomation,

            CatLitterBoxFan,
            CatLitterBoxFanAutomation,
            CirculatingPump,
            CirculatingPumpAutomation,
        }

        public StoreroomConfiguration(
            IAreaRegistryService areaService,
            IDeviceRegistryService deviceService,
            CCToolsDeviceService ccToolsBoardService,
            ITimerService timerService,
            ITwitterClientService twitterClientService,
            AutomationFactory automationFactory,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory,
            ILogService logService)
        {
            _areaService = areaService ?? throw new ArgumentNullException(nameof(areaService));
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _ccToolsBoardService = ccToolsBoardService ?? throw new ArgumentNullException(nameof(ccToolsBoardService));
            _timerService = timerService ?? throw new ArgumentNullException(nameof(timerService));
            _twitterClientService = twitterClientService ?? throw new ArgumentNullException(nameof(twitterClientService));
            _automationFactory = automationFactory ?? throw new ArgumentNullException(nameof(automationFactory));
            _actuatorFactory = actuatorFactory ?? throw new ArgumentNullException(nameof(actuatorFactory));
            _sensorFactory = sensorFactory ?? throw new ArgumentNullException(nameof(sensorFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public void Apply()
        {
            var hsrel8LowerHeatingValves = (HSREL8)_ccToolsBoardService.RegisterDevice(CCToolsDeviceType.HSRel8, InstalledDevice.LowerHeatingValvesHSREL8.ToString(), 16);
            var hsrel5UpperHeatingValves = (HSREL5)_ccToolsBoardService.RegisterDevice(CCToolsDeviceType.HSRel5, InstalledDevice.UpperHeatingValvesHSREL5.ToString(), 56);

            var hsrel5Stairway = _deviceService.GetDevice<HSREL5>(InstalledDevice.StairwayHSREL5.ToString());
            var input3 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input3.ToString());

            var room = _areaService.RegisterArea(Room.Storeroom);

            _actuatorFactory.RegisterLamp(room, Storeroom.LightCeiling, hsrel5Stairway[HSREL5Pin.GPIO1]);

            _sensorFactory.RegisterMotionDetector(room, Storeroom.MotionDetector, input3.GetInput(12));
            _sensorFactory.RegisterMotionDetector(room, Storeroom.MotionDetectorCatLitterBox, input3.GetInput(11));

            _actuatorFactory.RegisterSocket(room, Storeroom.CatLitterBoxFan, hsrel5Stairway[HSREL5Pin.GPIO2]);
            _actuatorFactory.RegisterSocket(room, Storeroom.CirculatingPump, hsrel5UpperHeatingValves[HSREL5Pin.Relay3]);

            _automationFactory.RegisterTurnOnAndOffAutomation(room, Storeroom.LightCeilingAutomation)
                .WithTrigger(room.GetMotionDetector(Storeroom.MotionDetector))
                .WithTarget(room.GetLamp(Storeroom.LightCeiling));

            _automationFactory.RegisterTurnOnAndOffAutomation(room, Storeroom.CatLitterBoxFan)
                .WithTrigger(room.GetMotionDetector(Storeroom.MotionDetectorCatLitterBox))
                .WithTarget(room.GetSocket(Storeroom.CatLitterBoxFan));

            // Both relays are used for water source selection (True+True = Lowerr, False+False = Upper)
            // Second relays is with capacitor. Disable second with delay before disable first one.
            hsrel5UpperHeatingValves[HSREL5Pin.GPIO0].Write(BinaryState.Low);
            hsrel5UpperHeatingValves[HSREL5Pin.GPIO1].Write(BinaryState.Low);

            _automationFactory.RegisterTurnOnAndOffAutomation(room, Storeroom.CirculatingPumpAutomation)
                .WithTrigger(_areaService.GetArea(Room.Kitchen).GetMotionDetector(KitchenConfiguration.Kitchen.MotionDetector))
                .WithTrigger(_areaService.GetArea(Room.LowerBathroom).GetMotionDetector(LowerBathroomConfiguration.LowerBathroom.MotionDetector))
                .WithTarget(room.GetSocket(Storeroom.CirculatingPump))
                .WithPauseAfterEveryTurnOn(TimeSpan.FromHours(1))
                .WithEnabledAtDay();

            _catLitterBoxTwitterSender =
                new CatLitterBoxTwitterSender(_timerService, _twitterClientService, _logService).WithTrigger(
                    room.GetMotionDetector(Storeroom.MotionDetectorCatLitterBox));
        }
    }
}
