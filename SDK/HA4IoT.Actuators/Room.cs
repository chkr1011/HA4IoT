using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Data.Json;
using HA4IoT.Actuators.Automations;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class Room
    {
        private readonly List<ActuatorBase> _ownActuators = new List<ActuatorBase>();

        public Room(Home home, string id)
        {
            if (home == null) throw new ArgumentNullException(nameof(home));

            Id = id;

            Home = home;
            Home.HttpApiController.Handle(HttpMethod.Get, "room").WithSegment(id).Using(c => c.Response.Body = new JsonBody(GetStatusAsJson()));
        }

        public string Id { get; }

        public Home Home { get; }

        public IReadOnlyCollection<ActuatorBase> Actuators => new ReadOnlyCollection<ActuatorBase>(_ownActuators);

        public Room WithActuator(Enum id, ActuatorBase actuator)
        {
            if (Home.Actuators.ContainsKey(id))
            {
                throw new InvalidOperationException("The actuator with ID " + id + " is aready registered.");
            }

            Home.Actuators.Add(id, actuator);
            _ownActuators.Add(actuator);
            return this;
        }

        public Room WithMotionDetector(Enum id, IBinaryInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            return WithActuator(id, new MotionDetector(GenerateActuatorId(id), input, Home.Timer, Home.HttpApiController, Home.NotificationHandler));
        }

        public Room WithWindow(Enum id, Action<Window> initializer)
        {
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));
            
            var window = new Window(GenerateActuatorId(id), Home.HttpApiController, Home.NotificationHandler);
            initializer(window);

            return WithActuator(id, window);
        }

        public Room WithLamp(Enum id, IBinaryOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            return WithActuator(id, new Lamp(GenerateActuatorId(id), output, Home.HttpApiController, Home.NotificationHandler));
        }

        public Room WithSocket(Enum id, IBinaryOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            return WithActuator(id, new Socket(GenerateActuatorId(id), output, Home.HttpApiController, Home.NotificationHandler));
        }

        public Room WithButton(Enum id, IBinaryInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            return WithActuator(id, new Button(GenerateActuatorId(id), input, Home.HttpApiController, Home.NotificationHandler, Home.Timer));
        }

        public Room WithVirtualButton(Enum id, Action<VirtualButton> initializer)
        {
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));

            var virtualButton = new VirtualButton(GenerateActuatorId(id), Home.HttpApiController, Home.NotificationHandler);
            initializer.Invoke(virtualButton);

            return WithActuator(id, virtualButton);
        }

        public Room WithVirtualButtonGroup(Enum id, Action<VirtualButtonGroup> initializer)
        {
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));

            var virtualButtonGroup = new VirtualButtonGroup(GenerateActuatorId(id), Home.HttpApiController, Home.NotificationHandler);
            initializer.Invoke(virtualButtonGroup);

            return WithActuator(id, virtualButtonGroup);
        }

        public Room WithRollerShutterButtons(Enum id, IBinaryInput upInput, IBinaryInput downInput)
        {
            if (upInput == null) throw new ArgumentNullException(nameof(upInput));
            if (downInput == null) throw new ArgumentNullException(nameof(downInput));

            return WithActuator(id, new RollerShutterButtons(GenerateActuatorId(id), upInput, downInput, Home.HttpApiController, Home.NotificationHandler, Home.Timer));
        }

        public Room WithHumiditySensor(Enum id, ISingleValueSensor sensor)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            return WithActuator(id,
                new HumiditySensor(GenerateActuatorId(id), sensor, Home.HttpApiController,
                    Home.NotificationHandler));
        }

        public Room WithTemperatureSensor(Enum id, ISingleValueSensor sensor)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            return WithActuator(id,
                new TemperatureSensor(GenerateActuatorId(id), sensor, Home.HttpApiController,
                    Home.NotificationHandler));
        }

        public Room WithStateMachine(Enum id, Action<StateMachine> initializer)
        {
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));

            var stateMachine = new StateMachine(GenerateActuatorId(id), Home.HttpApiController, Home.NotificationHandler);
            initializer(stateMachine);

            return WithActuator(id, stateMachine);
        }

        public StateMachine AddStateMachine(Enum id)
        {
            var actuator = new StateMachine(GenerateActuatorId(id), Home.HttpApiController, Home.NotificationHandler);
            WithActuator(id, actuator);
            return actuator;
        }

        public LogicalBinaryStateOutputActuator CombineActuators(Enum id)
        {
            var actuator = new LogicalBinaryStateOutputActuator(GenerateActuatorId(id), Home.HttpApiController,
                Home.NotificationHandler, Home.Timer);

            WithActuator(id, actuator);
            return actuator;
        }
        
        public AutomaticTurnOnAndOffAutomation SetupAutomaticTurnOnAndOffAction()
        {
            return new AutomaticTurnOnAndOffAutomation(Home.Timer);
        }

        public AutomaticConditionalOnAutomation SetupAlwaysOn()
        {
            return new AutomaticConditionalOnAutomation(Home.Timer);
        }

        public TActuator Actuator<TActuator>(Enum id) where TActuator : ActuatorBase
        {
            ActuatorBase actuator;
            if (!Home.Actuators.TryGetValue(id, out actuator))
            {
                throw new InvalidOperationException("The actuator with id '" + id + "' is not registered.");
            }

            return (TActuator)actuator;
        }

        public IBinaryStateOutputActuator BinaryStateOutput(Enum id)
        {
            return (IBinaryStateOutputActuator)Actuator<ActuatorBase>(id);
        }

        // TODO: Consider moving this methods to extension methods for each actuator. Button.cs and ButtonExtensions.cs (also place the connectors here)
        public Button Button(Enum id)
        {
            return Actuator<Button>(id);
        }

        public TemperatureSensor TemperatureSensor(Enum id)
        {
            return Actuator<TemperatureSensor>(id);
        }

        public HumiditySensor HumiditySensor(Enum id)
        {
            return Actuator<HumiditySensor>(id);
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

        public StateMachine StateMachine(Enum id)
        {
            return Actuator<StateMachine>(id);
        }

        public JsonObject GetConfigurationAsJson()
        {
            JsonArray actuatorDescriptions = new JsonArray();
            foreach (var actuator in _ownActuators)
            {
                JsonObject actuatorDescription = actuator.ApiGetConfiguration();
                actuatorDescriptions.Add(actuatorDescription);
            }

            JsonObject configuration = new JsonObject();
            configuration.SetNamedValue("actuators", actuatorDescriptions);
            return configuration;
        }

        public JsonObject GetStatusAsJson()
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

        public string GenerateActuatorId(Enum id)
        {
            return Id + "." + id;
        }
    }
}