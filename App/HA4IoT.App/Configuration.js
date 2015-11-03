// The interval which should be used to poll the current state from the Pi2.
appConfiguration.pollInterval = 250;

appConfiguration.showSensorsOverview = true;
appConfiguration.showRollerShuttersOverview = true;
appConfiguration.showMotionDetectorsOverview = true;
appConfiguration.showWindowsOverview = true;
appConfiguration.showWeatherStation = true;

// Supported values for an actuator configuration:
// caption:         The caption (the id is default)
// overviewCaption: The caption which is used at the overview
// image:           The name of the image
// sortValue:       The number which is used to sort the actuators
appConfiguration.actuatorExtender = function (actuator) {
    switch (actuator.id) {

        case "Floor.StairwayRollerShutter":
            {
                actuator.overviewCaption = "Flur";
                break;
            }

        case "Bedroom.WindowLeft":
            {
                actuator.caption = "Fenster links";
                actuator.overviewCaption = "Schlafzimmer links";
                break;
            }

        case "Bedroom.WindowRight":
            {
                actuator.caption = "Fenster rechts";
                actuator.overviewCaption = "Schlafzimmer rechts";
                break;
            }

        case "Bedroom.Fan":
            {
                actuator.image = "Fan";
                actuator.displayVertical = true;
                break;
            }

        case "Bedroom.SocketWindowLeft":
            {
                actuator.image = "Poison";
                break;
            }

        case "Bedroom.LampBedLeft":
        case "Bedroom.LampBedRight":
            {
                actuator.image = "Night-Lamp";
                break;
            }

        case "Office.CombinedCeilingLights":
            {
                actuator.displayVertical = true;
                actuator.image = "Lamp";
                break;
            }

        case "UpperBathroom.Fan":
            {
                actuator.image = "Air";
                break;
            }

        case "Storeroom.CirculatingPump":
            {
                actuator.image = "WaterPump";
                break;
            }

        case "Storeroom.CatLitterBoxFan":
            {
                actuator.image = "Cat";
                break;
            }

        case "Office.RemoteSocketDesk":
            {
                actuator.image = "Wifi";
                break;
            }

        case "Office.WindowLeft":
            {
                actuator.caption = "Fenster links";
                actuator.overviewCaption = "Büro links";
                break;
            }

        case "Bedroom.MotionDetector":
        case "Bedroom.TemperatureSensor":
        case "Bedroom.HumiditySensor":
            {
                actuator.overviewCaption = "Schlafzimmer";
                break;
            }

        case "Office.MotionDetector":
        case "Office.TemperatureSensor":
        case "Office.HumiditySensor":
            {
                actuator.overviewCaption = "Büro";
                break;
            }

        case "Garden.SocketPavillion":
            {
                actuator.caption = "Pavillion";
                break;
            }

        case "Garden.SpotlightRoof":
            {
                actuator.caption = "Strahler Dach";
                break;
            }

        case "Garden.LampTerrace":
            {
                actuator.caption = "Terasse";
                break;
            }

        case "Garden.LampTap":
            {
                actuator.caption = "Wasserhahn";
                break;
            }

        case "Garden.LampRearArea":
            {
                actuator.caption = "Tiere";
                break;
            }

        case "Garden.LampGarage":
            {
                actuator.caption = "Garage";
                break;
            }

        case "Garden.LampParkingLot1":
            {
                actuator.caption = "Parkplatz (1)";
                break;
            }

        case "Garden.LampParkingLot2":
            {
                actuator.caption = "Parkplatz (2)";
                break;
            }

        case "Garden.LampParkingLot3":
            {
                actuator.caption = "Parkplatz (3)";
                break;
            }

        case "Garden.StateMachine":
            {
                actuator.caption = "Vorlage";
                actuator.image = "Favorites";
                actuator.displayVertical = true;
                break;
            }

        case "ExampleRoom.CeilingFan":
            {
				actuator.displayVertical = true;
                actuator.image = "Fan";
                break;
            }

        case "ExampleRoom.BathroomFan":
            {
                actuator.image = "Air";
                break;
            }
    }
};

appConfiguration.roomExtender = function (room) {
    switch (room.id) {
        case "Bedroom":
            {
                room.sortValue = 1;
                break;
            }

        case "Office":
            {
                room.sortValue = 2;
                break;
            }
    }
}
