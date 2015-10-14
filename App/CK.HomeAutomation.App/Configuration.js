// The IP address of the Pi2 (only required if the app is not hosted at the Pi2).
appConfiguration.controllerAddress = "";
//appConfiguration.controllerAddress = "192.168.1.15";

// The interval which should be used to poll the current state from the Pi2.
appConfiguration.pollInterval = 250;

// Indicates whether the overview of all sensor (temperature and humidity) should be shown.
appConfiguration.showSensorsOverview = true;

// Indicates whether the overview of all roller shutters should be shown.
appConfiguration.showRollerShuttersOverview = true;

// Indicates whether the overview of all motion detectors should be shown.
appConfiguration.showMotionDetectorsOverview = true;

// Indicates whether the overview of all windows should be shown.
appConfiguration.showWindowsOverview = true;

// Indicates whether the values from the weather station should be shown.
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

        case "LivingRoom.WindowLeft":
            {
                actuator.caption = "Fenster / links";
                actuator.overviewCaption = "Wohnzimmer / links";
                break;
            }

        case "LivingRoom.WindowRight":
            {
                actuator.caption = "Fenster / rechts";
                actuator.overviewCaption = "Wohnzimmer / rechts";
                break;
            }

        case "Bedroom.RollerShutterLeft":
            {
                actuator.caption = "Rollo / links";
                actuator.overviewCaption = "Schlafzimmer / links";
                break;
            }

        case "Bedroom.RollerShutterRight":
            {
                actuator.caption = "Rollo / rechts";
                actuator.overviewCaption = "Schlafzimmer / rechts";
                break;
            }

        case "LivingRoom.MotionDetector":
        case "LivingRoom.TemperatureSensor":
        case "LivingRoom.HumiditySensor":
            {
                actuator.overviewCaption = "Wohnzimmer";
                break;
            }

        case "Kitchen.MotionDetector":
        case "Kitchen.TemperatureSensor":
        case "Kitchen.HumiditySensor":
            {
                actuator.overviewCaption = "Küche";
                break;
            }

        case "Bedroom.MotionDetector":
        case "Bedroom.TemperatureSensor":
        case "Bedroom.HumiditySensor":
            {
                actuator.overviewCaption = "Schlafzimmer";
                break;
            }

        case "ReadingRoom.Window":
        case "ReadingRoom.RollerShutter":
        case "ReadingRoom.TemperatureSensor":
        case "ReadingRoom.HumiditySensor":
            {
                actuator.overviewCaption = "Lesezimmer";
                break;
            }

        case "ChildrensRoom.Window":
        case "ChildrensRoom.RollerShutter":
        case "ChildrensRoom.TemperatureSensor":
        case "ChildrensRoom.HumiditySensor":
            {
                actuator.overviewCaption = "Kinderzimmer";
                break;
            }

        case "UpperBathroom.TemperatureSensor":
        case "UpperBathroom.HumiditySensor":
            {
                actuator.overviewCaption = "Bad / oben";
                break;
            }

        case "LowerBathroom.Window":
        case "LowerBathroom.TemperatureSensor":
        case "LowerBathroom.HumiditySensor":
            {
                actuator.overviewCaption = "Bad / unten";
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

friendlyNameLookup = [
	{ key: "Off", value: "Aus" },
    { key: "On", value: "An" },
    { key: "RollerShutter", value: "Rollo" },
    { key: "Window", value: "Fenster" },

    { key: "Bedroom", value: "Schlafzimmer" },
    { key: "LivingRoom", value: "Wohnzimmer" },
    { key: "Office", value: "Büro" },
    { key: "Kitchen", value: "Küche" },
    { key: "Floor", value: "Flur / Treppenhaus" },
    { key: "ChildrensRoom", value: "Kinderzimmer" },
    { key: "ReadingRoom", value: "Lesezimmer" },
    { key: "UpperBathroom", value: "Badezimmer (oben)" },
    { key: "LowerBathroom", value: "Badezimmer (unten)" },
    { key: "Storeroom", value: "Abstellkammer" },
    { key: "Garden", value: "Garten" },

    { key: "Bedroom.Fan", value: "Ventilator" },
    { key: "Bedroom.SocketWindowLeft", value: "Mückenstecker" },
    { key: "Bedroom.LightCeiling", value: "Licht" },
    { key: "Bedroom.LightCeilingWindow", value: "Strahler / Fenster" },
    { key: "Bedroom.LightCeilingWall", value: "Strahler / Wand" },
    { key: "Bedroom.SocketWindowRight", value: "LED-Band" },
    { key: "Bedroom.SocketWall", value: "Wand" },
    { key: "Bedroom.SocketWallEdge", value: "Ecke" },
    { key: "Bedroom.SocketBedLeft", value: "Bett / links" },
    { key: "Bedroom.SocketBedRight", value: "Bett / rechts" },
    { key: "Bedroom.LampBedRight", value: "Bett / rechts" },
    { key: "Bedroom.LampBedLeft", value: "Bett / links" },

    { key: "Office.LightCeilingFrontLeft", value: "Fenster links" },
    { key: "Office.LightCeilingFrontMiddle", value: "Fenster mitte" },
    { key: "Office.LightCeilingFrontRight", value: "Fenster rechts" },
    { key: "Office.LightCeilingMiddleLeft", value: "Mitte links" },
    { key: "Office.LightCeilingMiddleMiddle", value: "Mitte" },
    { key: "Office.LightCeilingMiddleRight", value: "Mitte rechts" },
    { key: "Office.LightCeilingRearLeft", value: "Schrank" },
    { key: "Office.LightCeilingRearRight", value: "Über Couch" },
    { key: "Office.SocketWindowLeft", value: "Fenster links" },
    { key: "Office.SocketWindowRight", value: "Fenster rechts" },
    { key: "Office.CombinedCeilingLights", value: "Vorlage" },
    { key: "Office.SocketFrontLeft", value: "Schräge links" },
    { key: "Office.SocketFrontRight", value: "Schräge rechts" },
    { key: "DeskOnly", value: "Schreibtisch" },
    { key: "CouchOnly", value: "Couch" },
    { key: "Office.RemoteSocketDesk", value: "Funksteckdose Schreibtisch" },
    { key: "Office.SocketRearLeft", value: "Wand links" },
    { key: "Office.SocketRearLeftEdge", value: "Wand links - Ecke" },
    { key: "Office.SocketRearRight", value: "Wand rechts" },

    { key: "ReadingRoom.LightCeilingMiddle", value: "Licht" },
    { key: "ReadingRoom.SocketWallLeft", value: "Schräge links" },
    { key: "ReadingRoom.SocketWallRight", value: "Schräge rechts" },
    { key: "ReadingRoom.SocketWindow", value: "Fenster" },

    { key: "ChildrensRoom.LightCeilingMiddle", value: "Licht" },
    { key: "ChildrensRoom.SocketWallLeft", value: "Schräge links" },
    { key: "ChildrensRoom.SocketWallRight", value: "Schräge rechts" },
    { key: "ChildrensRoom.SocketWindow", value: "Fenster" },

    { key: "UpperBathroom.LightCeilingDoor", value: "Tür" },
    { key: "UpperBathroom.LightCeilingEdge", value: "Ecke" },
    { key: "UpperBathroom.LightCeilingMirrorCabinet", value: "Über Spiegelschrank" },
    { key: "UpperBathroom.LampMirrorCabinet", value: "Spiegelschrank" },
    { key: "UpperBathroom.Fan", value: "Abzug" },

    { key: "LowerBathroom.LightCeilingDoor", value: "Tür" },
    { key: "LowerBathroom.LightCeilingMiddle", value: "Mitte" },
    { key: "LowerBathroom.LightCeilingWindow", value: "Fenster" },
    { key: "LowerBathroom.LampMirror", value: "Spiegel" },

    { key: "Storeroom.LightCeiling", value: "Licht" },
    { key: "Storeroom.CatLitterBoxFan", value: "Katzenklo-Lüftung" },
    { key: "Storeroom.CirculatingPump", value: "Zirkulationspumpe" },

    { key: "Kitchen.LightCeilingDoor", value: "Strahler / Tür" },
    { key: "Kitchen.LightCeilingMiddle", value: "Licht" },
    { key: "Kitchen.LightCeilingPassageInner", value: "Durchgang / innen" },
    { key: "Kitchen.LightCeilingPassageOuter", value: "Durchgang / außen" },
    { key: "Kitchen.LightCeilingWall", value: "Strahler / Wand" },
    { key: "Kitchen.LightCeilingWindow", value: "Strahler / Fenster" },
    { key: "Kitchen.SocketWall", value: "Wand" },

	{ key: "LivingRoom.LampCouch", value: "Couch" },
	{ key: "LivingRoom.LampDiningTable", value: "Esstisch" },
	{ key: "LivingRoom.SocketWallRightEdgeRight", value: "Stehlampe" },
	{ key: "LivingRoom.SocketWindowLeftLower", value: "Fenster l. unten" },
	{ key: "LivingRoom.SocketWindowLeftUpper", value: "Fenster l. oben" },
	{ key: "LivingRoom.SocketWindowMiddleLower", value: "Fenster m. unten" },
	{ key: "LivingRoom.SocketWindowRightLower", value: "Fenster r. unten" },
	{ key: "LivingRoom.SocketWindowRightUpper", value: "Fenster r. oben" },

    { key: "ExampleRoom.LedStripRemote", value: "LED Strip" },
    { key: "ExampleRoom.LedStripRemote.green1", value: "Green" },
    { key: "ExampleRoom.LedStripRemote.red1", value: "Red" },
    { key: "ExampleRoom.LedStripRemote.blue1", value: "Blue" },
    { key: "ExampleRoom.LedStripRemote.on", value: "An" },
    { key: "ExampleRoom.LedStripRemote.off", value: "Aus" },
    { key: "ExampleRoom.LedStripRemote.white", value: "White" },

    { key: "ExampleRoom.CeilingFan", value: "Ceiling fan" },
    { key: "ExampleRoom.CeilingFan.Off", value: "Stop" },
    { key: "ExampleRoom.CeilingFan.1", value: "Slow" },
    { key: "ExampleRoom.CeilingFan.2", value: "Fast" }
];
