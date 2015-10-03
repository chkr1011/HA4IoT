CREATE TABLE [PowerConsumption]
(
	[timestamp] DATETIME NOT NULL,
	[actuator] VARCHAR(50) NOT NULL,
	[state] VARCHAR(50) NOT NULL,
	[wattsPerHour] FLOAT NOT NULL,
	
	PRIMARY KEY ([actuator],[state])
)

CREATE INDEX PowerConsumption_timestampIndex ON [PowerConsumption]([timestamp])
CREATE INDEX PowerConsumption_actuatorIndex ON [PowerConsumption]([actuator])
CREATE INDEX PowerConsumption_stateIndex ON [PowerConsumption]([state])
CREATE INDEX PowerConsumption_wattsPerHourIndex ON [PowerConsumption]([wattsPerHour])

INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'UpperBathroom.LampMirrorCabinet', 'On', 14.5)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'UpperBathroom.LightCeilingMirrorCabinet', 'On', 3.5)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'UpperBathroom.LightCeilingEdge', 'On', 3.5)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'UpperBathroom.LightCeilingDoor', 'On', 3.5)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'UpperBathroom.Fan', '1', 10)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'UpperBathroom.Fan', '2', 13)

INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Bedroom.LightCeilingWindow', 'On', 10.5) --3x 3.5
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Bedroom.LightCeilingWall', 'On', 7) --2x 3.5
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Bedroom.LightCeiling', 'On', 10.5) --3x 3.5
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Bedroom.Fan', '1', 32)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Bedroom.Fan', '2', 40)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Bedroom.Fan', '3', 58)

INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'ChildrensRoom.LightCeilingMiddle', 'On', 15) --3x 5
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'ReadingRoom.LightCeilingMiddle', 'On', 15) --3x 5

INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Office.LightCeilingFrontLeft', 'On', 5)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Office.LightCeilingFrontMiddle', 'On', 5)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Office.LightCeilingFrontRight', 'On', 5)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Office.LightCeilingMiddleLeft', 'On', 5)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Office.LightCeilingMiddleMiddle', 'On', 18)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Office.LightCeilingMiddleRight', 'On', 5)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Office.LightCeilingRearLeft', 'On', 10.5) --3x 3.5
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Office.LightCeilingRearRight', 'On', 14) --4x 3.5

INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Floor.StairwayLampWall', 'On', 60)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Floor.StairwayLampCeiling', 'On', 60)

INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'LivingRoom.LampCouch', 'On', 15) --3x 5.0
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'LivingRoom.SocketWallRightEdgeRight', 'On', 37)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'LivingRoom.LampDiningTable', 'On', 10.1) -- 3 + 1.3 + 1.5 + 1.3 + 3

INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Kitchen.LightCeilingMiddle', 'On', 22) -- 2x 11
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Kitchen.LightCeilingWindow', 'On', 10.5) -- 3x 3.5
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Kitchen.LightCeilingWall', 'On', 7) -- 2x 3.5
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Kitchen.LightCeilingDoor', 'On', 3.5)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Kitchen.LightCeilingPassageOuter', 'On', 7) --2x 3.5
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Kitchen.LightCeilingPassageInner', 'On', 3.5)

INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Floor.LampStairs1', 'On', 0.3)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Floor.LampStairs2', 'On', 0.3)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Floor.LampStairs3', 'On', 0.3)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Floor.LampStairs4', 'On', 0.3)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Floor.LampStairs5', 'On', 0.3)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Floor.LampStairs6', 'On', 0.3)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Floor.LampStairsCeiling1', 'On', 3.5)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Floor.LampStairsCeiling2', 'On', 3.5)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Floor.LampStairsCeiling3', 'On', 3.5)
INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Floor.LampStairsCeiling4', 'On', 3.5)

INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Floor.LampStairsCeiling4', 'On', 3.5)

INSERT INTO [PowerConsumption] ([timestamp],[actuator],[state],[wattsPerHour]) VALUES ('01.01.2015 00:00:00', 'Storeroom.CirculatingPump', 'On', 25)