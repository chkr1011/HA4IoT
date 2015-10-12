using System;
using System.Collections.Generic;
using Windows.Data.Json;
using CK.HomeAutomation.Actuators.Automations;
using CK.HomeAutomation.Actuators.Contracts;
using CK.HomeAutomation.Hardware;
using CK.HomeAutomation.Networking;

namespace CK.HomeAutomation.Actuators
{
    public class Room
    {
        private readonly Home _home;
        private readonly List<ActuatorBase> _ownActuators = new List<ActuatorBase>();

        public Room(Home home, string id)
        {
            if (home == null) throw new ArgumentNullException(nameof(home));

            Id = id;

            _home = home;
            _home.HttpApiController.Handle(HttpMethod.Get, "room").WithSegment(id).Using(c => c.Response.Body = new JsonBody(GetStatusAsJSON()));
        }

        public string Id { get; }

        public Room WithActuator(Enum id, ActuatorBase actuator)
        {
            if (_home.Actuators.ContainsKey(id))
            {
                throw new InvalidOperationException("The actuator with ID " + id + " is aready registered.");
            }

            _home.Actuators.Add(id, actuator);
            _ownActuators.Add(actuator);
            return this;
        }

        public Room WithMotionDetector(Enum id, IBinaryInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            return WithActuator(id, new MotionDetector(GenerateID(id), input, _home.Timer, _home.HttpApiController, _home.NotificationHandler));
        }

        public Room WithWindow(Enum id, Action<Window> initializer)
        {
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));
            
            var window = new Window(GenerateID(id), _home.HttpApiController, _home.NotificationHandler);
            initializer(window);

            return WithActuator(id, window);
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

        public Room WithVirtualButton(Enum id, Action<VirtualButton> initializer)
        {
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));

            var virtualButton = new VirtualButton(GenerateID(id), _home.HttpApiController, _home.NotificationHandler);
            initializer.Invoke(virtualButton);

            return WithActuator(id, virtualButton);
        }

        public Room WithVirtualButtonGroup(Enum id, Action<VirtualButtonGroup> initializer)
        {
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));

            var virtualButtonGroup = new VirtualButtonGroup(GenerateID(id), _home.HttpApiController, _home.NotificationHandler);
            initializer.Invoke(virtualButtonGroup);

            return WithActuator(id, virtualButtonGroup);
        }

        public Room WithRollerShutterButtons(Enum id, IBinaryInput upInput, IBinaryInput downInput)
        {
            if (upInput == null) throw new ArgumentNullException(nameof(upInput));
            if (downInput == null) throw new ArgumentNullException(nameof(downInput));

            return WithActuator(id, new RollerShutterButtons(GenerateID(id), upInput, downInput, _home.HttpApiController, _home.NotificationHandler, _home.Timer));
        }

        public Room WithHumiditySensor(Enum id, ISingleValueSensor sensor)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            return WithActuator(id,
                new HumiditySensor(GenerateID(id), sensor, _home.HttpApiController,
                    _home.NotificationHandler));
        }

        public Room WithTemperatureSensor(Enum id, ISingleValueSensor sensor)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            return WithActuator(id,
                new TemperatureSensor(GenerateID(id), sensor, _home.HttpApiController,
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
                _home.NotificationHandler, _home.Timer);

            WithActuator(id, actuator);
            return actuator;
        }

        public AutomaticRollerShutterAutomation SetupAutomaticRollerShutters()
        {
            return new AutomaticRollerShutterAutomation(_home.Timer, _home.WeatherStation);
        }

        public AutomaticTurnOnAndOffAutomation SetupAutomaticTurnOnAndOffAction()
        {
            return new AutomaticTurnOnAndOffAutomation(_home.Timer);
        }

        public AutomaticConditionalOnAutomation SetupAlwaysOn()
        {
            return new AutomaticConditionalOnAutomation(_home.Timer);
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
                JsonObject actuatorDescription = actuator.ApiGetConfiguration();
                actuatorDescriptions.Add(actuatorDescription);
            }

            JsonObject configuration = new JsonObject();
            configuration.SetNamedValue("id", JsonValue.CreateStringValue(Id));
            configuration.SetNamedValue("actuators", actuatorDescriptions);
            return configuration;
        }

        public TActuator Actuator<TActuator>(Enum id) where TActuator : ActuatorBase
        {
            ActuatorBase actuator;
            if (!_home.Actuators.TryGetValue(id, out actuator))
            {
                throw new InvalidOperationException("The actuator with id '" + id + "' is not registered.");
            }

            return (TActuator)actuator;
        }

        public IBinaryStateOutputActuator BinaryStateOutput(Enum id)
        {
            return (IBinaryStateOutputActuator)Actuator<ActuatorBase>(id);
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