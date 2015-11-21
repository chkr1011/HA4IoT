var exampleConfig = {
  "Bedroom": {
    "id": "Bedroom",
    "actuators": [
      {
        "id": "Bedroom.TemperatureSensor",
        "type": "HA4IoT.Actuators.TemperatureSensor"
      },
      {
        "id": "Bedroom.HumiditySensor",
        "type": "HA4IoT.Actuators.HumiditySensor"
      },
      {
        "id": "Bedroom.MotionDetector",
        "type": "HA4IoT.Actuators.MotionDetector"
      },
      {
        "id": "Bedroom.LightCeiling",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Bedroom.LightCeilingWindow",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Bedroom.LightCeilingWall",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Bedroom.SocketWindowLeft",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "Bedroom.SocketWindowRight",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "Bedroom.SocketWall",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "Bedroom.SocketWallEdge",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "Bedroom.SocketBedLeft",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "Bedroom.SocketBedRight",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "Bedroom.LampBedLeft",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Bedroom.LampBedRight",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Bedroom.ButtonDoor",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "Bedroom.ButtonWindowUpper",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "Bedroom.ButtonWindowLower",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "Bedroom.ButtonBedLeftInner",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "Bedroom.ButtonBedLeftOuter",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "Bedroom.ButtonBedRightInner",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "Bedroom.ButtonBedRightOuter",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "Bedroom.RollerShutterLeft",
        "type": "HA4IoT.Actuators.RollerShutter"
      },
      {
        "id": "Bedroom.RollerShutterRight",
        "type": "HA4IoT.Actuators.RollerShutter"
      },
      {
        "id": "Bedroom.RollerShutterButtonsUpper",
        "type": "HA4IoT.Actuators.RollerShutterButtons"
      },
      {
        "id": "Bedroom.RollerShutterButtonsLower",
        "type": "HA4IoT.Actuators.RollerShutterButtons"
      },
      {
        "id": "Bedroom.WindowLeft",
        "type": "HA4IoT.Actuators.Window",
        "casements": [
          "Center"
        ]
      },
      {
        "id": "Bedroom.WindowRight",
        "type": "HA4IoT.Actuators.Window",
        "casements": [
          "Center"
        ]
      },
      {
        "id": "Bedroom.CombinedCeilingLights",
        "type": "HA4IoT.Actuators.LogicalBinaryStateOutputActuator"
      },
      {
        "id": "Bedroom.Fan",
        "type": "HA4IoT.Actuators.StateMachine",
        "states": [
          "Off",
          "1",
          "2",
          "3"
        ]
      }
    ]
  },
  "Office": {
    "id": "Office",
    "actuators": [
      {
        "id": "Office.MotionDetector",
        "type": "HA4IoT.Actuators.MotionDetector"
      },
      {
        "id": "Office.TemperatureSensor",
        "type": "HA4IoT.Actuators.TemperatureSensor"
      },
      {
        "id": "Office.HumiditySensor",
        "type": "HA4IoT.Actuators.HumiditySensor"
      },
      {
        "id": "Office.LightCeilingFrontRight",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Office.LightCeilingFrontMiddle",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Office.LightCeilingFrontLeft",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Office.LightCeilingMiddleRight",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Office.LightCeilingMiddleMiddle",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Office.LightCeilingMiddleLeft",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Office.LightCeilingRearRight",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Office.LightCeilingRearLeft",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Office.SocketFrontLeft",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "Office.SocketFrontRight",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "Office.SocketWindowLeft",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "Office.SocketWindowRight",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "Office.SocketRearLeftEdge",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "Office.SocketRearLeft",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "Office.SocketRearRight",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "Office.ButtonUpperLeft",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "Office.ButtonLowerLeft",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "Office.ButtonLowerRight",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "Office.ButtonUpperRight",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "Office.WindowLeft",
        "type": "HA4IoT.Actuators.Window",
        "casements": [
          "Left",
          "Right"
        ]
      },
      {
        "id": "Office.WindowRight",
        "type": "HA4IoT.Actuators.Window",
        "casements": [
          "Left",
          "Right"
        ]
      },
      {
        "id": "Office.RemoteSocketDesk",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "Office.CombinedCeilingLightsCouchOnly",
        "type": "HA4IoT.Actuators.LogicalBinaryStateOutputActuator"
      },
      {
        "id": "Office.CombinedCeilingLightsDeskOnly",
        "type": "HA4IoT.Actuators.LogicalBinaryStateOutputActuator"
      },
      {
        "id": "Office.CombinedCeilingLightsOther",
        "type": "HA4IoT.Actuators.LogicalBinaryStateOutputActuator"
      },
      {
        "id": "Office.CombinedCeilingLights",
        "type": "HA4IoT.Actuators.StateMachine",
        "states": [
          "Off",
          "On",
          "DeskOnly",
          "CouchOnly"
        ]
      }
    ]
  },
  "UpperBathroom": {
    "id": "UpperBathroom",
    "actuators": [
      {
        "id": "UpperBathroom.TemperatureSensor",
        "type": "HA4IoT.Actuators.TemperatureSensor"
      },
      {
        "id": "UpperBathroom.HumiditySensor",
        "type": "HA4IoT.Actuators.HumiditySensor"
      },
      {
        "id": "UpperBathroom.MotionDetector",
        "type": "HA4IoT.Actuators.MotionDetector"
      },
      {
        "id": "UpperBathroom.LightCeilingDoor",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "UpperBathroom.LightCeilingEdge",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "UpperBathroom.LightCeilingMirrorCabinet",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "UpperBathroom.LampMirrorCabinet",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "UpperBathroom.CombinedCeilingLights",
        "type": "HA4IoT.Actuators.LogicalBinaryStateOutputActuator"
      },
      {
        "id": "UpperBathroom.Fan",
        "type": "HA4IoT.Actuators.StateMachine",
        "states": [
          "Off",
          "1",
          "2"
        ]
      }
    ]
  },
  "ReadingRoom": {
    "id": "ReadingRoom",
    "actuators": [
      {
        "id": "ReadingRoom.TemperatureSensor",
        "type": "HA4IoT.Actuators.TemperatureSensor"
      },
      {
        "id": "ReadingRoom.HumiditySensor",
        "type": "HA4IoT.Actuators.HumiditySensor"
      },
      {
        "id": "ReadingRoom.LightCeilingMiddle",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "ReadingRoom.RollerShutter",
        "type": "HA4IoT.Actuators.RollerShutter"
      },
      {
        "id": "ReadingRoom.SocketWindow",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "ReadingRoom.SocketWallLeft",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "ReadingRoom.SocketWallRight",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "ReadingRoom.Button",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "ReadingRoom.Window",
        "type": "HA4IoT.Actuators.Window",
        "casements": [
          "Center"
        ]
      },
      {
        "id": "ReadingRoom.RollerShutterButtons",
        "type": "HA4IoT.Actuators.RollerShutterButtons"
      }
    ]
  },
  "ChildrensRoom": {
    "id": "ChildrensRoom",
    "actuators": [
      {
        "id": "ChildrensRoom.TemperatureSensor",
        "type": "HA4IoT.Actuators.TemperatureSensor"
      },
      {
        "id": "ChildrensRoom.HumiditySensor",
        "type": "HA4IoT.Actuators.HumiditySensor"
      },
      {
        "id": "ChildrensRoom.LightCeilingMiddle",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "ChildrensRoom.RollerShutter",
        "type": "HA4IoT.Actuators.RollerShutter"
      },
      {
        "id": "ChildrensRoom.SocketWindow",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "ChildrensRoom.SocketWallLeft",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "ChildrensRoom.SocketWallRight",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "ChildrensRoom.Button",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "ChildrensRoom.Window",
        "type": "HA4IoT.Actuators.Window",
        "casements": [
          "Center"
        ]
      },
      {
        "id": "ChildrensRoom.RollerShutterButtons",
        "type": "HA4IoT.Actuators.RollerShutterButtons"
      }
    ]
  },
  "Kitchen": {
    "id": "Kitchen",
    "actuators": [
      {
        "id": "Kitchen.TemperatureSensor",
        "type": "HA4IoT.Actuators.TemperatureSensor"
      },
      {
        "id": "Kitchen.HumiditySensor",
        "type": "HA4IoT.Actuators.HumiditySensor"
      },
      {
        "id": "Kitchen.MotionDetector",
        "type": "HA4IoT.Actuators.MotionDetector"
      },
      {
        "id": "Kitchen.LightCeilingMiddle",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Kitchen.LightCeilingWindow",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Kitchen.LightCeilingWall",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Kitchen.LightCeilingDoor",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Kitchen.LightCeilingPassageInner",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Kitchen.LightCeilingPassageOuter",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Kitchen.SocketWall",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "Kitchen.RollerShutter",
        "type": "HA4IoT.Actuators.RollerShutter"
      },
      {
        "id": "Kitchen.ButtonKitchenette",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "Kitchen.ButtonPassage",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "Kitchen.RollerShutterButtons",
        "type": "HA4IoT.Actuators.RollerShutterButtons"
      },
      {
        "id": "Kitchen.Window",
        "type": "HA4IoT.Actuators.Window",
        "casements": [
          "Center"
        ]
      },
      {
        "id": "Kitchen.CombinedAutomaticLights",
        "type": "HA4IoT.Actuators.LogicalBinaryStateOutputActuator"
      }
    ]
  },
  "Floor": {
    "id": "Floor",
    "actuators": [
      {
        "id": "Floor.StairwayMotionDetector",
        "type": "HA4IoT.Actuators.MotionDetector"
      },
      {
        "id": "Floor.StairsLowerMotionDetector",
        "type": "HA4IoT.Actuators.MotionDetector"
      },
      {
        "id": "Floor.StairsUpperMotionDetector",
        "type": "HA4IoT.Actuators.MotionDetector"
      },
      {
        "id": "Floor.LowerFloorMotionDetector",
        "type": "HA4IoT.Actuators.MotionDetector"
      },
      {
        "id": "Floor.LowerFloorTemperatureSensor",
        "type": "HA4IoT.Actuators.TemperatureSensor"
      },
      {
        "id": "Floor.LowerFloorHumiditySensor",
        "type": "HA4IoT.Actuators.HumiditySensor"
      },
      {
        "id": "Floor.Lamp1",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Floor.Lamp2",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Floor.Lamp3",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Floor.StairwayLampCeiling",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Floor.StairwayLampWall",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Floor.StairwayRollerShutter",
        "type": "HA4IoT.Actuators.RollerShutter"
      },
      {
        "id": "Floor.ButtonLowerFloorUpper",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "Floor.ButtonLowerFloorLower",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "Floor.ButtonLowerFloorAtBathroom",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "Floor.ButtonLowerFloorAtKitchen",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "Floor.ButtonStairsLowerUpper",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "Floor.ButtonStairsLowerLower",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "Floor.ButtonStairsUpper",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "Floor.ButtonStairway",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "Floor.CombinedStairwayLamp",
        "type": "HA4IoT.Actuators.LogicalBinaryStateOutputActuator"
      },
      {
        "id": "Floor.CombinedLamps",
        "type": "HA4IoT.Actuators.LogicalBinaryStateOutputActuator"
      },
      {
        "id": "Floor.LampStairsCeiling1",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Floor.LampStairsCeiling2",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Floor.LampStairsCeiling3",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Floor.LampStairsCeiling4",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Floor.CombinedLampStairsCeiling",
        "type": "HA4IoT.Actuators.LogicalBinaryStateOutputActuator"
      },
      {
        "id": "Floor.LampStairs1",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Floor.LampStairs2",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Floor.LampStairs3",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Floor.LampStairs4",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Floor.LampStairs5",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Floor.LampStairs6",
        "type": "HA4IoT.Actuators.Lamp"
      }
    ]
  },
  "LowerBathroom": {
    "id": "LowerBathroom",
    "actuators": [
      {
        "id": "LowerBathroom.MotionDetector",
        "type": "HA4IoT.Actuators.MotionDetector"
      },
      {
        "id": "LowerBathroom.TemperatureSensor",
        "type": "HA4IoT.Actuators.TemperatureSensor"
      },
      {
        "id": "LowerBathroom.HumiditySensor",
        "type": "HA4IoT.Actuators.HumiditySensor"
      },
      {
        "id": "LowerBathroom.LightCeilingDoor",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "LowerBathroom.LightCeilingMiddle",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "LowerBathroom.LightCeilingWindow",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "LowerBathroom.LampMirror",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "LowerBathroom.Window",
        "type": "HA4IoT.Actuators.Window",
        "casements": [
          "Center"
        ]
      },
      {
        "id": "LowerBathroom.CombinedLights",
        "type": "HA4IoT.Actuators.LogicalBinaryStateOutputActuator"
      }
    ]
  },
  "Storeroom": {
    "id": "Storeroom",
    "actuators": [
      {
        "id": "Storeroom.MotionDetector",
        "type": "HA4IoT.Actuators.MotionDetector"
      },
      {
        "id": "Storeroom.LightCeiling",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "Storeroom.CatLitterBoxFan",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "Storeroom.CirculatingPump",
        "type": "HA4IoT.Actuators.Socket"
      }
    ]
  },
  "LivingRoom": {
    "id": "LivingRoom",
    "actuators": [
      {
        "id": "LivingRoom.TemperatureSensor",
        "type": "HA4IoT.Actuators.TemperatureSensor"
      },
      {
        "id": "LivingRoom.HumiditySensor",
        "type": "HA4IoT.Actuators.HumiditySensor"
      },
      {
        "id": "LivingRoom.LampCouch",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "LivingRoom.LampDiningTable",
        "type": "HA4IoT.Actuators.Lamp"
      },
      {
        "id": "LivingRoom.SocketWindowLeftLower",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "LivingRoom.SocketWindowMiddleLower",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "LivingRoom.SocketWindowRightLower",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "LivingRoom.SocketWallRightEdgeRight",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "LivingRoom.SocketWindowLeftUpper",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "LivingRoom.SocketWindowRightUpper",
        "type": "HA4IoT.Actuators.Socket"
      },
      {
        "id": "LivingRoom.ButtonUpper",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "LivingRoom.ButtonMiddle",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "LivingRoom.ButtonLower",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "LivingRoom.ButtonPassage",
        "type": "HA4IoT.Actuators.Button"
      },
      {
        "id": "LivingRoom.WindowLeft",
        "type": "HA4IoT.Actuators.Window",
        "casements": [
          "Left",
          "Right"
        ]
      },
      {
        "id": "LivingRoom.WindowRight",
        "type": "HA4IoT.Actuators.Window",
        "casements": [
          "Left",
          "Right"
        ]
      }
    ]
  }
};

module.exports = exampleConfig;
