USE [ckha-db]
GO

-- The total on durations of all actuators (no state machines).
SELECT [actuator],SUM(duration)/60 as [minutes]
FROM [dbo].[OutputActuatorStateChangedEvents]
WHERE Kind = 'End' AND [state] = 'On'
GROUP BY actuator
ORDER BY actuator

-- The average temperature.
SELECT [actuator],AVG(value) as [temperature]
FROM [dbo].[SensorValueChangedEvents]
WHERE Kind = 'TemperatureSensor'
GROUP BY actuator
ORDER BY actuator

-- The average humidity.
SELECT [actuator],AVG(value) as [humidity]
FROM [dbo].[SensorValueChangedEvents]
WHERE Kind = 'HumiditySensor'
GROUP BY actuator
ORDER BY actuator

-- The count of motion detections-
SELECT [actuator],COUNT(1) as [detections]
FROM [dbo].[MotionDetectedEvents]
GROUP BY [actuator]
ORDER BY [detections] DESC
