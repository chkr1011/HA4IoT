// The interval which should be used to poll the current state from the controller in ms.
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
                actuator.image = "HA4IoT.Actuators.Lamp";
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

        case "LivingRoom.SocketWallRightEdgeRight":
            {
                actuator.image = "HA4IoT.Actuators.Lamp";
                break;
            }

        case "LivingRoom.SocketWallLeftEdgeLeft":
            {
                actuator.image = "HA4IoT.Actuators.Lamp";
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