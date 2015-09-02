controllerAddress = "192.168.1.15";
pollInterval = 500;

// Supported values for an actuator configuration:
// caption: "The caption (the id is default)
// image: "The name of the image"
// sortValue: 1 (The value which is used to sort the actuators) 
actuatorExtender = function(actuator) {
    if (actuator.id === "Bedroom.Fan") {
        actuator.image = "Fan";
        actuator.displayVertical = true;
    } else if (actuator.id === "Bedroom.SocketWindowLeft") {
        actuator.image = "Poison";
    } else if (actuator.id === "Office.CombinedCeilingLights") {
        actuator.displayVertical = true;
        actuator.image = "Lamp";
    } else if (actuator.id === "UpperBathroom.Fan") {
        actuator.image = "Air";
    } else if (actuator.id === "Storeroom.CirculatingPump") {
        actuator.image = "WaterPump";
    } else if (actuator.id === "Storeroom.CatLitterBoxFan") {
        actuator.image = "Cat";
    }
};

roomExtender = function(room) {
    
}

friendlyNameLookup = [
    { key: "Off", value: "Aus" },
    { key: "On", value: "An" },

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

    { key: "Bedroom.RollerShutterLeft", value: "Rollo links" },
    { key: "Bedroom.RollerShutterRight", value: "Rollo rechts" },
    { key: "Bedroom.Fan", value: "Ventilator" },
    { key: "Bedroom.SocketWindowLeft", value: "Mückenstecker" },
    { key: "Bedroom.LightCeiling", value: "Decke (mitte)" },
    { key: "Bedroom.LightCeilingWindow", value: "Fenster" },
    { key: "Bedroom.LightCeilingWall", value: "Wand" },
    { key: "Bedroom.SocketWindowRight", value: "LED-Band" },
    { key: "Bedroom.SocketWall", value: "Wand" },
    { key: "Bedroom.SocketWallEdge", value: "Ecke" },
    { key: "Bedroom.SocketBedLeft", value: "Bett links" },
    { key: "Bedroom.SocketBedRight", value: "Bett rechts" },
    { key: "Bedroom.LampBedRight", value: "Bett rechts" },
    { key: "Bedroom.LampBedLeft", value: "Bett links" },

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
    { key: "Storeroom.CirculatingPump", value: "Zirkulationspumpe" }





            //SocketFrontLeft,
            //SocketFrontRight,
            //SocketRearRight,
            //SocketRearLeft,
            //SocketRearLeftEdge,

            //ButtonUpperLeft,
            //ButtonUpperRight,
            //ButtonLowerLeft,
            //ButtonLowerRight,

            //CombinedCeilingLights,
            //CombinedCeilingLightsCouchOnly,
            //CombinedCeilingLightsDeskOnly,
            //CombinedCeilingLightsOther
];