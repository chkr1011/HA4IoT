using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Automations;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.ExternalServices.Twitter;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
using HA4IoT.PersonalAgent;
using HA4IoT.Sensors;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Devices;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class StoreroomConfiguration
    {
        private readonly IAreaService _areaService;
        private readonly SynonymService _synonymService;
        private readonly IDeviceService _deviceService;
        private readonly CCToolsBoardService _ccToolsBoardService;
        private readonly ITimerService _timerService;
        private readonly ITwitterClientService _twitterClientService;
        private readonly AutomationFactory _automationFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly SensorFactory _sensorFactory;
        private CatLitterBoxTwitterSender _catLitterBoxTwitterSender;

        private enum Storeroom
        {
            MotionDetector,
            MotionDetectorCatLitterBox,
            LightCeiling,

            CatLitterBoxFan,
            CirculatingPump
        }

        public StoreroomConfiguration(
            IAreaService areaService,
            SynonymService synonymService,
            IDeviceService deviceService,
            CCToolsBoardService ccToolsBoardService,
            ITimerService timerService,
            ITwitterClientService twitterClientService,
            AutomationFactory automationFactory,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory)
        {
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            if (synonymService == null) throw new ArgumentNullException(nameof(synonymService));
            if (deviceService == null) throw new ArgumentNullException(nameof(deviceService));
            if (ccToolsBoardService == null) throw new ArgumentNullException(nameof(ccToolsBoardService));
            if (timerService == null) throw new ArgumentNullException(nameof(timerService));
            if (twitterClientService == null) throw new ArgumentNullException(nameof(twitterClientService));
            if (automationFactory == null) throw new ArgumentNullException(nameof(automationFactory));
            if (actuatorFactory == null) throw new ArgumentNullException(nameof(actuatorFactory));
            if (sensorFactory == null) throw new ArgumentNullException(nameof(sensorFactory));

            _areaService = areaService;
            _synonymService = synonymService;
            _deviceService = deviceService;
            _ccToolsBoardService = ccToolsBoardService;
            _timerService = timerService;
            _twitterClientService = twitterClientService;
            _automationFactory = automationFactory;
            _actuatorFactory = actuatorFactory;
            _sensorFactory = sensorFactory;
        }

        public void Apply()
        {
            var hsrel8LowerHeatingValves = _ccToolsBoardService.RegisterHSREL8(InstalledDevice.LowerHeatingValvesHSREL8, new I2CSlaveAddress(16));
            var hsrel5UpperHeatingValves = _ccToolsBoardService.RegisterHSREL5(InstalledDevice.UpperHeatingValvesHSREL5, new I2CSlaveAddress(56));

            var hsrel5Stairway = _deviceService.GetDevice<HSREL5>(InstalledDevice.StairwayHSREL5);
            var input3 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input3);

            var room = _areaService.CreateArea(Room.Storeroom);

            _actuatorFactory.RegisterLamp(room, Storeroom.LightCeiling, hsrel5Stairway[HSREL5Pin.GPIO1]);

            _sensorFactory.RegisterMotionDetector(room, Storeroom.MotionDetector, input3.GetInput(12));
            _sensorFactory.RegisterMotionDetector(room, Storeroom.MotionDetectorCatLitterBox, input3.GetInput(11).WithInvertedState());

            _actuatorFactory.RegisterSocket(room, Storeroom.CatLitterBoxFan, hsrel5Stairway[HSREL5Pin.GPIO2]);
            _actuatorFactory.RegisterSocket(room, Storeroom.CirculatingPump, hsrel5UpperHeatingValves[HSREL5Pin.Relay3]);

            _automationFactory.RegisterTurnOnAndOffAutomation(room)
                .WithTrigger(room.GetMotionDetector(Storeroom.MotionDetector))
                .WithTarget(room.GetLamp(Storeroom.LightCeiling));

            _automationFactory.RegisterTurnOnAndOffAutomation(room)
                .WithTrigger(room.GetMotionDetector(Storeroom.MotionDetectorCatLitterBox))
                .WithTarget(room.GetSocket(Storeroom.CatLitterBoxFan));

            // Both relays are used for water source selection (True+True = Lowerr, False+False = Upper)
            // Second relays is with capacitor. Disable second with delay before disable first one.
            hsrel5UpperHeatingValves[HSREL5Pin.GPIO0].Write(BinaryState.Low);
            hsrel5UpperHeatingValves[HSREL5Pin.GPIO1].Write(BinaryState.Low);

            _automationFactory.RegisterTurnOnAndOffAutomation(room)
                .WithTrigger(_areaService.GetArea(Room.Kitchen).GetMotionDetector(KitchenConfiguration.Kitchen.MotionDetector))
                .WithTrigger(_areaService.GetArea(Room.LowerBathroom).GetMotionDetector(LowerBathroomConfiguration.LowerBathroom.MotionDetector))
                .WithTarget(room.GetSocket(Storeroom.CirculatingPump))
                .WithPauseAfterEveryTurnOn(TimeSpan.FromHours(1))
                .WithEnabledAtDay();

            _catLitterBoxTwitterSender =
                new CatLitterBoxTwitterSender(_timerService, _twitterClientService).WithTrigger(
                    room.GetMotionDetector(Storeroom.MotionDetectorCatLitterBox));

            _synonymService.AddSynonymsForArea(Room.Storeroom, "Abstellkammer", "Storeroom");
        }
    }
}
