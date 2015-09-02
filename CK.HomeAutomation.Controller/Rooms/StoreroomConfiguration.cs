using System;
using CK.HomeAutomation.Actuators;
using CK.HomeAutomation.Hardware.CCTools;
using CK.HomeAutomation.Hardware.Drivers;

namespace CK.HomeAutomation.Controller.Rooms
{
    internal class StoreroomConfiguration
    {
        private enum Storeroom
        {
            MotionDetector,
            LightCeiling,

            CatLitterBoxFan,
            CirculatingPump
        }

        public void Setup(Home home, IOBoardManager ioBoardManager, CCToolsFactory ccToolsFactory, TemperatureAndHumiditySensorBridgeDriver sensorBridgeDriver)
        {
            var hsrel8LowerHeatingValves = ccToolsFactory.CreateHSREL8(Device.LowerHeatingValvesHSREL8, 16);
            var hsrel5UpperHeatingValves = ccToolsFactory.CreateHSREL5(Device.UpperHeatingValvesHSREL5, 56);

            var hsrel5Stairway = ioBoardManager.GetOutputBoard(Device.StairwayHSREL5);
            var input3 = ioBoardManager.GetInputBoard(Device.Input3);

            var storeroom = home.AddRoom(Room.Storeroom)
                .WithMotionDetector(Storeroom.MotionDetector, input3.GetInput(12))
                .WithLamp(Storeroom.LightCeiling, hsrel5Stairway.GetOutput(7).WithInvertedState())
                .WithSocket(Storeroom.CatLitterBoxFan, hsrel8LowerHeatingValves.GetOutput(15));

            storeroom.SetupAutomaticTurnOnAction()
                .WithMotionDetector(storeroom.MotionDetector(Storeroom.MotionDetector))
                .WithTarget(storeroom.Lamp(Storeroom.LightCeiling));

            storeroom.WithSocket(Storeroom.CirculatingPump, hsrel5UpperHeatingValves.GetOutput(3));

            storeroom.SetupAutomaticTurnOnAction()
                .WithMotionDetector(home.Room(Room.Kitchen).MotionDetector(KitchenConfiguration.Kitchen.MotionDetector))
                .WithMotionDetector(home.Room(Room.LowerBathroom).MotionDetector(LowerBathroomConfiguration.LowerBathroom.MotionDetector))
                .WithTarget(storeroom.Socket(Storeroom.CirculatingPump))
                .WithOnDuration(TimeSpan.FromMinutes(1))
                .WithOnlyAtDayTimeRange(home.WeatherStation);
        }
    }
}
