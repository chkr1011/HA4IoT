using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Scripting;
using MoonSharp.Interpreter;

namespace HA4IoT.Components
{
    public class ComponentRegistryScriptProxy : IScriptProxy
    {
        private readonly IScriptingSession _scriptingSession;
        private readonly IComponentRegistryService _componentRegistry;

        [MoonSharpHidden]
        public ComponentRegistryScriptProxy(IComponentRegistryService componentRegistry, IScriptingSession scriptingSession)
        {
            _scriptingSession = scriptingSession ?? throw new ArgumentNullException(nameof(scriptingSession));
            _componentRegistry = componentRegistry ?? throw new ArgumentNullException(nameof(componentRegistry));
        }

        [MoonSharpHidden]
        public string Name => "component";

        public void TogglePowerState(string id)
        {
            _componentRegistry.GetComponent(id).TryTogglePowerState();
        }

        public void TurnOn(string id)
        {
            _componentRegistry.GetComponent(id).TryTurnOn();
        }

        public void TurnOff(string id)
        {
            _componentRegistry.GetComponent(id).TryTurnOff();
        }

        public void MoveUp(string id)
        {
            _componentRegistry.GetComponent(id).TryMoveUp();
        }

        public void MoveDown(string id)
        {
            _componentRegistry.GetComponent(id).TryMoveDown();
        }

        public void IncreaseLevel(string id)
        {
            _componentRegistry.GetComponent(id).TryIncreaseLevel();
        }

        public void DecreaseLevel(string id)
        {
            _componentRegistry.GetComponent(id).TryDecreaseLevel();
        }

        public void SetLevel(string id, int level)
        {
            _componentRegistry.GetComponent(id).TrySetLevel(level);
        }

        public void SetColor(string id, double h, double s, double v)
        {
            _componentRegistry.GetComponent(id).TrySetColor(h, s, v);
        }

        public void SetState(string id, string state)
        {
            _componentRegistry.GetComponent(id).TrySetState(state);
        }

        public void SetNextState(string id)
        {
            _componentRegistry.GetComponent(id).TrySetNextState();
        }

        public string GetState(string id)
        {
            string v;
            _componentRegistry.GetComponent(id).TryGetState(out v);
            return v;
        }

        public float? GetTemperature(string id)
        {
            float? t;
            _componentRegistry.GetComponent(id).TryGetTemperature(out t);
            return t;
        }

        public float? GetHumidity(string id)
        {
            float? h;
            _componentRegistry.GetComponent(id).TryGetHumidity(out h);
            return h;
        }

        public string GetMotionDetectionState(string id)
        {
            MotionDetectionStateValue v;
            _componentRegistry.GetComponent(id).TryGetMotionDetectionState(out v);
            return v.ToString();
        }

        public string GetButtonState(string id)
        {
            ButtonStateValue v;
            _componentRegistry.GetComponent(id).TryGetButtonState(out v);
            return v.ToString();
        }
    }
}
