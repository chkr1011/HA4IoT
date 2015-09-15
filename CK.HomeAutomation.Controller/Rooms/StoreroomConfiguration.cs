using System;
using CK.HomeAutomation.Actuators;
using CK.HomeAutomation.Hardware.CCTools;
using CK.HomeAutomation.Hardware.DHT22;
using CK.HomeAutomation.Hardware.GenericIOBoard;

namespace CK.HomeAutomation.Controller.Rooms
{
    internal class StoreroomConfiguration
    {
        public void Setup(Home home, CCToolsBoardController ccToolsController, IOBoardManager ioBoardManager, DHT22Reader dht22Reader)
        {
            var hsrel8LowerHeatingValves = ccToolsController.CreateHSREL8(Device.LowerHeatingValvesHSREL8, 16);
            var hsrel5UpperHeatingValves = ccToolsController.CreateHSREL5(Device.UpperHeatingValvesHSREL5, 56);

            var hsrel5Stairway = ioBoardManager.GetOutputBoard(Device.StairwayHSREL5);
            var input3 = ioBoardManager.GetInputBoard(Device.Input3);

            var storeroom = home.AddRoom(Room.Storeroom)
                .WithMotionDetector(Storeroom.MotionDetector, input3.GetInput(12))
                .WithLamp(Storeroom.LightCeiling, hsrel5Stairway.GetOutput(7).WithInvertedState())
                .WithSocket(Storeroom.CatLitterBoxFan, hsrel8LowerHeatingValves.GetOutput(15));

            storeroom.SetupAutomaticTurnOnAndOffAction()
                .WithMotionDetector(storeroom.MotionDetector(Storeroom.MotionDetector))
                .WithTarget(storeroom.Lamp(Storeroom.LightCeiling))
                .WithTarget(storeroom.Socket(Storeroom.CatLitterBoxFan))
                .WithOnDuration(TimeSpan.FromMinutes(1));

            storeroom.WithSocket(Storeroom.CirculatingPump, hsrel5UpperHeatingValves.GetOutput(3));

            storeroom.SetupAutomaticTurnOnAndOffAction()
                .WithMotionDetector(home.Room(Room.Kitchen).MotionDetector(KitchenConfiguration.Kitchen.MotionDetector))
                .WithMotionDetector(home.Room(Room.LowerBathroom).MotionDetector(LowerBathroomConfiguration.LowerBathroom.MotionDetector))
                .WithTarget(storeroom.Socket(Storeroom.CirculatingPump))
                .WithOnDuration(TimeSpan.FromMinutes(1))
                .WithOnAtDayTimeRange(home.WeatherStation);
        }

        private enum Storeroom
        {
            MotionDetector,
            LightCeiling,

            CatLitterBoxFan,
            CirculatingPump
        }
    }
}
