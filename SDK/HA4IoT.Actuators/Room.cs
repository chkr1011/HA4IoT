using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Data.Json;
using HA4IoT.Actuators.Automations;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class Room
    {
        private readonly List<ActuatorBase> _actuators = new List<ActuatorBase>();

        public Room(Home home, string id)
        {
            if (home == null) throw new ArgumentNullException(nameof(home));

            Id = id;

            Home = home;
            Home.HttpApiController.Handle(HttpMethod.Get, "room").WithSegment(id).Using(HandleApiGet);
        }

        public string Id { get; }

        public Home Home { get; }

        public IReadOnlyCollection<ActuatorBase> Actuators => new ReadOnlyCollection<ActuatorBase>(_actuators);

        public Room WithActuator(Enum id, ActuatorBase actuator)
        {
            if (Home.Actuators.ContainsKey(id))
            {
                throw new InvalidOperationException("The actuator with ID " + id + " is aready registered.");
            }

            Home.Actuators.Add(id, actuator);
            _actuators.Add(actuator);

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

            var lamp = new Lamp(GenerateActuatorId(id), output, Home.HttpApiController, Home.NotificationHandler);
            lamp.SetInitialState();

            return WithActuator(id, lamp);
        }

        public Room WithSocket(Enum id, IBinaryOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            var socket = new Socket(GenerateActuatorId(id), output, Home.HttpApiController, Home.NotificationHandler);
            socket.SetInitialState();

            return WithActuator(id, socket);
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

        public Room WithStateMachine(Enum id, Action<StateMachine, Room> initializer)
        {
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));

            var stateMachine = new StateMachine(GenerateActuatorId(id), Home.HttpApiController, Home.NotificationHandler);
            initializer(stateMachine, this);
            stateMachine.SetInitialState();

            return WithActuator(id, stateMachine);
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

        public ITemperatureSensor TemperatureSensor(Enum id)
        {
            return Actuator<TemperatureSensor>(id);
        }

        public IHumiditySensor HumiditySensor(Enum id)
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

        public IMotionDetector MotionDetector(Enum id)
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
            foreach (var actuator in _actuators)
            {
                JsonObject actuatorDescription = actuator.GetConfiguration();
                actuatorDescriptions.Add(actuatorDescription);
            }

            JsonObject configuration = new JsonObject();
            configuration.SetNamedValue("actuators", actuatorDescriptions);
            return configuration;
        }

        public string GenerateActuatorId(Enum id)
        {
            return Id + "." + id;
        }

        private void HandleApiGet(HttpContext httpContext)
        {
            JsonObject state = new JsonObject();
            foreach (var actuator in _actuators)
            {
                var context = new ApiRequestContext(new JsonObject(), new JsonObject());
                actuator.HandleApiGet(context);
                state.SetNamedValue(actuator.Id, context.Response);
            }

            httpContext.Response.Body = new JsonBody(state);
        }
    }
}