using System;
using System.Collections.Generic;
using Windows.Data.Json;
using CK.HomeAutomation.Actuators.Automations;
using CK.HomeAutomation.Hardware;
using CK.HomeAutomation.Hardware.DHT22;
using CK.HomeAutomation.Networking;

namespace CK.HomeAutomation.Actuators
{
    public class Room
    {
        private readonly Home _home;
        private readonly List<BaseActuator> _ownActuators = new List<BaseActuator>();

        public Room(Home home, string id)
        {
            if (home == null) throw new ArgumentNullException(nameof(home));

            Id = id;

            _home = home;
            _home.HttpApiController.Handle(HttpMethod.Get, "room").WithSegment(id).Using(c => c.Response.Result = GetStatusAsJSON());
        }

        public string Id { get; }

        public Room WithActuator(Enum id, BaseActuator actuator)
        {
            _home.Actuators.Add(id, actuator);
            _ownActuators.Add(actuator);
            return this;
        }

        public Room WithMotionDetector(Enum id, IBinaryInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            return WithActuator(id, new MotionDetector(GenerateID(id), input, _home.Timer, _home.HttpApiController, _home.NotificationHandler));
        }

        public Room WithLamp(Enum id, IBinaryOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            return WithActuator(id, new Lamp(GenerateID(id), output, _home.HttpApiController, _home.NotificationHandler));
        }

        public Room WithSocket(Enum id, IBinaryOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            return WithActuator(id, new Socket(GenerateID(id), output, _home.HttpApiController, _home.NotificationHandler));
        }

        public Room WithRollerShutter(Enum id, IBinaryOutput powerOutput, IBinaryOutput directionOutput,
            TimeSpan autoOffTimeout, int maxPosition)
        {
            if (powerOutput == null) throw new ArgumentNullException(nameof(powerOutput));
            if (directionOutput == null) throw new ArgumentNullException(nameof(directionOutput));

            return WithActuator(id, new RollerShutter(GenerateID(id), powerOutput, directionOutput, autoOffTimeout, maxPosition, _home.HttpApiController, _home.NotificationHandler, _home.Timer));
        }

        public Room WithButton(Enum id, IBinaryInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            return WithActuator(id, new Button(GenerateID(id), input, _home.HttpApiController, _home.NotificationHandler, _home.Timer));
        }

        public Room WithRollerShutterButtons(Enum id, IBinaryInput upInput, IBinaryInput downInput)
        {
            if (upInput == null) throw new ArgumentNullException(nameof(upInput));
            if (downInput == null) throw new ArgumentNullException(nameof(downInput));

            return WithActuator(id, new RollerShutterButtons(GenerateID(id), upInput, downInput, _home.HttpApiController, _home.NotificationHandler, _home.Timer));
        }

        public Room WithHumiditySensor(Enum id, int sensorId, DHT22Reader sensorReader)
        {
            if (sensorReader == null) throw new ArgumentNullException(nameof(sensorReader));

            return WithActuator(id,
                new HumiditySensor(GenerateID(id), sensorId, sensorReader, _home.HttpApiController,
                    _home.NotificationHandler));
        }

        public Room WithTemperatureSensor(Enum id, int sensorId, DHT22Reader sensorReader)
        {
            if (sensorReader == null) throw new ArgumentNullException(nameof(sensorReader));

            return WithActuator(id,
                new TemperatureSensor(GenerateID(id), sensorId, sensorReader, _home.HttpApiController,
                    _home.NotificationHandler));
        }

        public StateMachine AddStateMachine(Enum id)
        {
            var actuator = new StateMachine(GenerateID(id), _home.HttpApiController, _home.NotificationHandler);
            WithActuator(id, actuator);
            return actuator;
        }

        public CombinedBinaryStateActuators CombineActuators(Enum id)
        {
            var actuator = new CombinedBinaryStateActuators(GenerateID(id), _home.HttpApiController,
                _home.NotificationHandler);

            WithActuator(id, actuator);
            return actuator;
        }

        public AutomaticRollerShutterAutomation SetupAutomaticRollerShutters()
        {
            return new AutomaticRollerShutterAutomation(_home.Timer, _home.WeatherStation);
        }

        public AutomaticTurnOnAutomation SetupAutomaticTurnOnAction()
        {
            return new AutomaticTurnOnAutomation(_home.Timer);
        }

        public AlwaysOnAutomation SetupAlwaysOn()
        {
            return new AlwaysOnAutomation(_home.Timer);
        }

        public JsonObject GetStatusAsJSON()
        {
            JsonObject state = new JsonObject();
            foreach (var actuator in _ownActuators)
            {
                var context = new ApiRequestContext(new JsonObject(), new JsonObject());
                actuator.ApiGet(context);
                state.SetNamedValue(actuator.Id, context.Response);
            }

            return state;
        }

        public JsonObject GetConfigurationAsJSON()
        {
            JsonArray actuatorDescriptions = new JsonArray();
            foreach (var actuator in _ownActuators)
            {
                var actuatorDescription = new JsonObject();
                actuatorDescription.SetNamedValue("id", JsonValue.CreateStringValue(actuator.Id));
                actuatorDescription.SetNamedValue("type", JsonValue.CreateStringValue(actuator.GetType().Name));

                var stateMachine = actuator as StateMachine;
                if (stateMachine != null)
                {
                    JsonArray stateMachineStates = new JsonArray();
                    foreach (StateMachineState state in stateMachine.States)
                    {
                        stateMachineStates.Add(JsonValue.CreateStringValue(state.Id));
                    }

                    actuatorDescription.SetNamedValue("states", stateMachineStates);
                }

                actuatorDescriptions.Add(actuatorDescription);
            }

            JsonObject configuration = new JsonObject();
            configuration.SetNamedValue("id", JsonValue.CreateStringValue(Id));
            configuration.SetNamedValue("actuators", actuatorDescriptions);
            return configuration;
        }

        public TActuator Actuator<TActuator>(Enum id) where TActuator : BaseActuator
        {
            BaseActuator actuator;
            if (!_home.Actuators.TryGetValue(id, out actuator))
            {
                throw new InvalidOperationException("The actuator with id '" + id + "' is not registered.");
            }

            return (TActuator)actuator;
        }

        public IBinaryStateOutputActuator BinaryStateOutput(Enum id)
        {
            return (IBinaryStateOutputActuator)Actuator<BaseActuator>(id);
        }

        public Button Button(Enum id)
        {
            return Actuator<Button>(id);
        }

        public Lamp Lamp(Enum id)
        {
            return Actuator<Lamp>(id);
        }

        public Socket Socket(Enum id)
        {
            return Actuator<Socket>(id);
        }

        public RollerShutterButtons RollerShutterButtons(Enum id)
        {
            return Actuator<RollerShutterButtons>(id);
        }

        public MotionDetector MotionDetector(Enum id)
        {
            return Actuator<MotionDetector>(id);
        }

        public RollerShutter RollerShutter(Enum id)
        {
            return Actuator<RollerShutter>(id);
        }

        public StateMachine StateMachine(Enum id)
        {
            return Actuator<StateMachine>(id);
        }

        private string GenerateID(Enum id)
        {
            return Id + "." + id;
        }
    }
}