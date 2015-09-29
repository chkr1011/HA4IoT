-- Create the table for all SensorValueChanged events.
CREATE TABLE SensorValueChangedEvents
(
	[id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[actuator] VARCHAR(50) NOT NULL,
	[timestamp] DATETIME NOT NULL,
	[kind] VARCHAR(50) NOT NULL, -- Temperature, Humidity
	[value] FLOAT NOT NULL
)

GO

CREATE INDEX [SensorValueChangedEvents_ActuatorIndex] ON SensorValueChangedEvents([actuator])
CREATE INDEX [SensorValueChangedEvents_TimestampIndex] ON SensorValueChangedEvents([timestamp])
CREATE INDEX [SensorValueChangedEvents_KindIndex] ON SensorValueChangedEvents([kind])
CREATE INDEX [SensorValueChangedEvents_ValueIndex] ON SensorValueChangedEvents([value])

GO

-- Create the table for all BinaryStateOutputActuatorStateChanged events.
CREATE TABLE OutputActuatorStateChangedEvents
(
	[id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[actuator] VARCHAR(50) NOT NULL,
	[timestamp] DATETIME NOT NULL,
	[kind] VARCHAR(50) NOT NULL, -- Start, End
	[state] VARCHAR(50) NOT NULL,
	[duration] BIGINT
)

GO

CREATE INDEX [OutputActuatorStateChangedEvents_ActuatorIndex] ON OutputActuatorStateChangedEvents([actuator])
CREATE INDEX [OutputActuatorStateChangedEvents_TimestampIndex] ON OutputActuatorStateChangedEvents([timestamp])
CREATE INDEX [OutputActuatorStateChangedEvents_KindIndex] ON OutputActuatorStateChangedEvents([kind])
CREATE INDEX [OutputActuatorStateChangedEvents_StateIndex] ON OutputActuatorStateChangedEvents([state])
CREATE INDEX [OutputActuatorStateChangedEvents_DurationIndex] ON OutputActuatorStateChangedEvents([duration])

GO

-- Create the table for all ButtonPressed events.
CREATE TABLE ButtonPressedEvents
(
	[id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[actuator] VARCHAR(50) NOT NULL,
	[timestamp] DATETIME NOT NULL,
	[kind] VARCHAR(50) NOT NULL, -- Short, Long
)

GO

CREATE INDEX [ButtonPressedEvents_ActuatorIndex] ON ButtonPressedEvents([actuator])
CREATE INDEX [ButtonPressedEvents_TimestampIndex] ON ButtonPressedEvents([timestamp])
CREATE INDEX [ButtonPressedEvents_KindIndex] ON ButtonPressedEvents([kind])

-- Create the table for all MotionDetected events.
CREATE TABLE MotionDetectedEvents
(
	[id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[actuator] VARCHAR(50) NOT NULL,
	[timestamp] DATETIME NOT NULL,
	[kind] VARCHAR(50) NOT NULL, -- Detected, DetectionCompleted
)

GO

CREATE INDEX [MotionDetected_ActuatorIndex] ON MotionDetectedEvents([actuator])
CREATE INDEX [MotionDetected_TimestampIndex] ON MotionDetectedEvents([timestamp])
CREATE INDEX [MotionDetected_KindIndex] ON MotionDetectedEvents([kind])

GO