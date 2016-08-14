using System;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Automations;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
using HA4IoT.PersonalAgent;
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
        private readonly IDaylightService _daylightService;
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
            IDaylightService daylightService)
        {
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            if (synonymService == null) throw new ArgumentNullException(nameof(synonymService));
            if (deviceService == null) throw new ArgumentNullException(nameof(deviceService));
            if (ccToolsBoardService == null) throw new ArgumentNullException(nameof(ccToolsBoardService));
            if (timerService == null) throw new ArgumentNullException(nameof(timerService));
            if (daylightService == null) throw new ArgumentNullException(nameof(daylightService));

            _areaService = areaService;
            _synonymService = synonymService;
            _deviceService = deviceService;
            _ccToolsBoardService = ccToolsBoardService;
            _timerService = timerService;
            _daylightService = daylightService;
        }

        public void Setup()
        {
            var hsrel8LowerHeatingValves = _ccToolsBoardService.CreateHSREL8(InstalledDevice.LowerHeatingValvesHSREL8, new I2CSlaveAddress(16));
            var hsrel5UpperHeatingValves = _ccToolsBoardService.CreateHSREL5(InstalledDevice.UpperHeatingValvesHSREL5, new I2CSlaveAddress(56));

            var hsrel5Stairway = _deviceService.GetDevice<HSREL5>(InstalledDevice.StairwayHSREL5);
            var input3 = _deviceService.GetDevice<HSPE16InputOnly>(InstalledDevice.Input3);

            var storeroom = _areaService.CreateArea(Room.Storeroom)
                .WithMotionDetector(Storeroom.MotionDetector, input3.GetInput(12))
                .WithMotionDetector(Storeroom.MotionDetectorCatLitterBox, input3.GetInput(11).WithInvertedState())
                .WithLamp(Storeroom.LightCeiling, hsrel5Stairway[HSREL5Pin.GPIO1])
                .WithSocket(Storeroom.CatLitterBoxFan, hsrel5Stairway[HSREL5Pin.GPIO2]);

            storeroom.SetupTurnOnAndOffAutomation()
                .WithTrigger(storeroom.GetMotionDetector(Storeroom.MotionDetector))
                .WithTarget(storeroom.GetLamp(Storeroom.LightCeiling))
                .WithOnDuration(TimeSpan.FromMinutes(1));

            storeroom.SetupTurnOnAndOffAutomation()
                .WithTrigger(storeroom.GetMotionDetector(Storeroom.MotionDetectorCatLitterBox))
                .WithTarget(storeroom.Socket(Storeroom.CatLitterBoxFan))
                .WithOnDuration(TimeSpan.FromMinutes(2));

            storeroom.WithSocket(Storeroom.CirculatingPump, hsrel5UpperHeatingValves[HSREL5Pin.Relay3]);
            
            // Both relays are used for water source selection (True+True = Lowerr, False+False = Upper)
            // Second relays is with capacitor. Disable second with delay before disable first one.
            hsrel5UpperHeatingValves[HSREL5Pin.GPIO0].Write(BinaryState.Low);
            hsrel5UpperHeatingValves[HSREL5Pin.GPIO1].Write(BinaryState.Low);

            storeroom.SetupTurnOnAndOffAutomation()
                .WithTrigger(_areaService.GetArea(Room.Kitchen).GetMotionDetector(KitchenConfiguration.Kitchen.MotionDetector))
                .WithTrigger(_areaService.GetArea(Room.LowerBathroom).GetMotionDetector(LowerBathroomConfiguration.LowerBathroom.MotionDetector))
                .WithTarget(storeroom.Socket(Storeroom.CirculatingPump))
                .WithPauseAfterEveryTurnOn(TimeSpan.FromHours(1))
                .WithOnDuration(TimeSpan.FromMinutes(1))
                .WithEnabledAtDay(_daylightService);

            _catLitterBoxTwitterSender =
                new CatLitterBoxTwitterSender(_timerService).WithTrigger(
                    storeroom.GetMotionDetector(Storeroom.MotionDetectorCatLitterBox));

            _synonymService.AddSynonymsForArea(Room.Storeroom, "Abstellkammer", "Storeroom");
        }
    }
}
