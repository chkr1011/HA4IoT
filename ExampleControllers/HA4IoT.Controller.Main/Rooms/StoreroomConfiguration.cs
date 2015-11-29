using System;
using HA4IoT.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.GenericIOBoard;

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

        public void Setup(Home home, CCToolsBoardController ccToolsController, IOBoardCollection ioBoardManager)
        {
            var hsrel8LowerHeatingValves = ccToolsController.CreateHSREL8(Device.LowerHeatingValvesHSREL8, new I2CSlaveAddress(16));
            var hsrel5UpperHeatingValves = ccToolsController.CreateHSREL5(Device.UpperHeatingValvesHSREL5, new I2CSlaveAddress(56));

            var hsrel5Stairway = ioBoardManager.GetOutputBoard(Device.StairwayHSREL5);
            var input3 = ioBoardManager.GetInputBoard(Device.Input3);

            var storeroom = home.AddRoom(Room.Storeroom)
                .WithMotionDetector(Storeroom.MotionDetector, input3.GetInput(12))
                .WithMotionDetector(Storeroom.MotionDetectorCatLitterBox, input3.GetInput(11).WithInvertedState())
                .WithLamp(Storeroom.LightCeiling, hsrel5Stairway.GetOutput(7).WithInvertedState())
                .WithSocket(Storeroom.CatLitterBoxFan, hsrel8LowerHeatingValves.GetOutput(15));

            storeroom.SetupAutomaticTurnOnAndOffAction()
                .WithTrigger(storeroom.MotionDetector(Storeroom.MotionDetector))
                .WithTarget(storeroom.Lamp(Storeroom.LightCeiling))
                .WithOnDuration(TimeSpan.FromMinutes(1));

            storeroom.SetupAutomaticTurnOnAndOffAction()
                .WithTrigger(storeroom.MotionDetector(Storeroom.MotionDetectorCatLitterBox))
                .WithTarget(storeroom.Socket(Storeroom.CatLitterBoxFan))
                .WithOnDuration(TimeSpan.FromMinutes(2));

            storeroom.WithSocket(Storeroom.CirculatingPump, hsrel5UpperHeatingValves.GetOutput(3));

            storeroom.SetupAutomaticTurnOnAndOffAction()
                .WithTrigger(home.Room(Room.Kitchen).MotionDetector(KitchenConfiguration.Kitchen.MotionDetector))
                .WithTrigger(home.Room(Room.LowerBathroom).MotionDetector(LowerBathroomConfiguration.LowerBathroom.MotionDetector))
                .WithTarget(storeroom.Socket(Storeroom.CirculatingPump))
                .WithOnDuration(TimeSpan.FromMinutes(1))
                .WithEnabledAtDay(home.WeatherStation);

            _catLitterBoxTwitterSender =
                new CatLitterBoxTwitterSender(home.Timer, home.Log).WithTrigger(
                    storeroom.MotionDetector(Storeroom.MotionDetectorCatLitterBox));
        }
    }
}
