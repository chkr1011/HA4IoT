using System;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Automations;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Core;
using HA4IoT.Hardware.CCTools;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class StoreroomConfiguration
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

        public void Setup(Controller controller, CCToolsBoardController ccToolsController)
        {
            var hsrel8LowerHeatingValves = ccToolsController.CreateHSREL8(Device.LowerHeatingValvesHSREL8, new I2CSlaveAddress(16));
            var hsrel5UpperHeatingValves = ccToolsController.CreateHSREL5(Device.UpperHeatingValvesHSREL5, new I2CSlaveAddress(56));

            var hsrel5Stairway = controller.GetDevice<HSREL5>(Device.StairwayHSREL5);
            var input3 = controller.GetDevice<HSPE16InputOnly>(Device.Input3);

            var storeroom = controller.CreateRoom(Room.Storeroom)
                .WithMotionDetector(Storeroom.MotionDetector, input3.GetInput(12))
                .WithMotionDetector(Storeroom.MotionDetectorCatLitterBox, input3.GetInput(11).WithInvertedState())
                .WithLamp(Storeroom.LightCeiling, hsrel5Stairway.GetOutput(7).WithInvertedState())
                .WithSocket(Storeroom.CatLitterBoxFan, hsrel8LowerHeatingValves.GetOutput(15));

            storeroom.SetupAutomaticTurnOnAndOffAutomation()
                .WithTrigger(storeroom.MotionDetector(Storeroom.MotionDetector))
                .WithTarget(storeroom.Lamp(Storeroom.LightCeiling))
                .WithOnDuration(TimeSpan.FromMinutes(1));

            storeroom.SetupAutomaticTurnOnAndOffAutomation()
                .WithTrigger(storeroom.MotionDetector(Storeroom.MotionDetectorCatLitterBox))
                .WithTarget(storeroom.Socket(Storeroom.CatLitterBoxFan))
                .WithOnDuration(TimeSpan.FromMinutes(2));

            storeroom.WithSocket(Storeroom.CirculatingPump, hsrel5UpperHeatingValves.GetOutput(3));

            // TODO: Create RoomIdFactory like ActuatorIdFactory.
            storeroom.SetupAutomaticTurnOnAndOffAutomation()
                .WithTrigger(controller.Room(new RoomId(Room.Kitchen.ToString())).MotionDetector(KitchenConfiguration.Kitchen.MotionDetector))
                .WithTrigger(controller.Room(new RoomId(Room.LowerBathroom.ToString())).MotionDetector(LowerBathroomConfiguration.LowerBathroom.MotionDetector))
                .WithTarget(storeroom.Socket(Storeroom.CirculatingPump))
                .WithPauseAfterEveryTurnOn(TimeSpan.FromHours(1))
                .WithOnDuration(TimeSpan.FromMinutes(1))
                .WithEnabledAtDay(controller.WeatherStation);

            _catLitterBoxTwitterSender =
                new CatLitterBoxTwitterSender(controller.Timer, controller.Logger).WithTrigger(
                    storeroom.MotionDetector(Storeroom.MotionDetectorCatLitterBox));
        }
    }
}
