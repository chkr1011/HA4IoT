SELECT [actuator],[timestamp],[kind],CAST([value] AS FLOAT) as [value]
INTO [DB-SensorValueChangedEvents]
FROM [CKHA]
WHERE [type] = 'SensorValueChanged'

SELECT [actuator],[timestamp],[kind],[state],CAST([duration] AS BIGINT) as [duration]
INTO [DB-OutputActuatorStateChangedEvents]
FROM [CKHA]
WHERE [type] = 'OutputActuatorStateChanged'

SELECT [actuator],[timestamp],[kind]
INTO [DB-ButtonPressedEvents]
FROM [CKHA]
WHERE [type] = 'ButtonPressed'

SELECT [actuator],[timestamp],[kind]
INTO [DB-MotionDetectedEvents]
FROM [CKHA]
WHERE [type] = 'MotionDetected'