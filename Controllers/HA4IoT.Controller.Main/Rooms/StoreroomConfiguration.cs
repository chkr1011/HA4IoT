using System;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Automations;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services;
using HA4IoT.Core;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.PersonalAgent;
using HA4IoT.Sensors.MotionDetectors;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class StoreroomConfiguration : RoomConfiguration
    {
        private CatLitterBoxTwitterSender _catLitterBoxTwitterSender;

        private enum Storeroom
        {
            MotionDetector,
            MotionDetectorCatLitterBox,
            LightCeiling,

            CatLitterBoxFan,
            CirculatingPump
        }

        public StoreroomConfiguration(IController controller) 
            : base(controller)
        {
        }

        public override void Setup()
        {
            var hsrel8LowerHeatingValves = CCToolsBoardController.CreateHSREL8(InstalledDevice.LowerHeatingValvesHSREL8, new I2CSlaveAddress(16));
            var hsrel5UpperHeatingValves = CCToolsBoardController.CreateHSREL5(InstalledDevice.UpperHeatingValvesHSREL5, new I2CSlaveAddress(56));

            var hsrel5Stairway = Controller.Device<HSREL5>(InstalledDevice.StairwayHSREL5);
            var input3 = Controller.Device<HSPE16InputOnly>(InstalledDevice.Input3);

            var storeroom = Controller.CreateArea(Room.Storeroom)
                .WithMotionDetector(Storeroom.MotionDetector, input3.GetInput(12))
                .WithMotionDetector(Storeroom.MotionDetectorCatLitterBox, input3.GetInput(11).WithInvertedState())
                .WithLamp(Storeroom.LightCeiling, hsrel5Stairway.GetOutput(7).WithInvertedState())
                .WithSocket(Storeroom.CatLitterBoxFan, hsrel8LowerHeatingValves.GetOutput(15));

            storeroom.SetupTurnOnAndOffAutomation()
                .WithTrigger(storeroom.GetMotionDetector(Storeroom.MotionDetector))
                .WithTarget(storeroom.GetLamp(Storeroom.LightCeiling))
                .WithOnDuration(TimeSpan.FromMinutes(1));

            storeroom.SetupTurnOnAndOffAutomation()
                .WithTrigger(storeroom.GetMotionDetector(Storeroom.MotionDetectorCatLitterBox))
                .WithTarget(storeroom.Socket(Storeroom.CatLitterBoxFan))
                .WithOnDuration(TimeSpan.FromMinutes(2));

            storeroom.WithSocket(Storeroom.CirculatingPump, hsrel5UpperHeatingValves.GetOutput(3));

            // TODO: Create RoomIdFactory like ActuatorIdFactory.
            storeroom.SetupTurnOnAndOffAutomation()
                .WithTrigger(Controller.GetArea(new AreaId(Room.Kitchen.ToString())).GetMotionDetector(KitchenConfiguration.Kitchen.MotionDetector))
                .WithTrigger(Controller.GetArea(new AreaId(Room.LowerBathroom.ToString())).GetMotionDetector(LowerBathroomConfiguration.LowerBathroom.MotionDetector))
                .WithTarget(storeroom.Socket(Storeroom.CirculatingPump))
                .WithPauseAfterEveryTurnOn(TimeSpan.FromHours(1))
                .WithOnDuration(TimeSpan.FromMinutes(1))
                .WithEnabledAtDay(Controller.ServiceLocator.GetService<IDaylightService>());

            _catLitterBoxTwitterSender =
                new CatLitterBoxTwitterSender(Controller.Timer).WithTrigger(
                    storeroom.GetMotionDetector(Storeroom.MotionDetectorCatLitterBox));

            Controller.ServiceLocator.GetService<SynonymService>().AddSynonymsForArea(Room.Storeroom, "Abstellkammer", "Storeroom");
        }
    }
}
